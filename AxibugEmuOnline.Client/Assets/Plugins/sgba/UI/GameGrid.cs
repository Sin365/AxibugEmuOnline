using System.Collections;

namespace sGBA;

internal sealed class GameGridLayout
{
	private float _itemWidth = 100f;
	private float _itemHeight = 100f;
	private Sandbox.UI.Box _box;
	private float _scaleFromScreen;
	private float _scrollOffset;
	private Vector2 _cellSize;
	private int _columns;
	private int _updateHash;

	public float ItemWidth { get => _itemWidth; set => _itemWidth = MathF.Max( 1f, value ); }
	public float ItemHeight { get => _itemHeight; set => _itemHeight = MathF.Max( 1f, value ); }
	public Vector2 Spacing { get; set; } = 0;

	public bool Update( Sandbox.UI.Box box, float scaleFromScreen, float scrollOffset )
	{
		var hash = HashCode.Combine( box.RectInner, scaleFromScreen, scrollOffset, _itemWidth, _itemHeight, Spacing );
		if ( hash == _updateHash ) return false;
		_updateHash = hash;

		_box = box;
		_scaleFromScreen = scaleFromScreen;
		_scrollOffset = scrollOffset;
		_cellSize = new Vector2( _itemWidth, _itemHeight );

		var inner = box.RectInner;
		inner.Position = box.RectInner.Position - box.Rect.Position;
		var rect = inner * scaleFromScreen;

		float stepX = _cellSize.x + Spacing.x;
		_columns = stepX > 0f ? ((rect.Width + Spacing.x) / stepX).FloorToInt() : 1;
		if ( _columns < 1 ) _columns = 1;

		float totalSpacing = (_columns - 1) * Spacing.x;
		_cellSize.x = (rect.Width - totalSpacing) / _columns;
		_cellSize.y = MathF.Max( 1f, _cellSize.x * (_itemHeight / _itemWidth) );

		return true;
	}

	public void GetVisibleRange( out int first, out int pastEnd )
	{
		var inner = _box.RectInner;
		inner.Position = _box.RectInner.Position - _box.Rect.Position;
		var rect = inner * _scaleFromScreen;
		var outerRect = _box.Rect * _scaleFromScreen;

		float rowStep = MathF.Max( 1f, _cellSize.y + Spacing.y );
		int topRow = ((_scrollOffset - rect.Top) / rowStep).FloorToInt();
		if ( topRow < 0 ) topRow = 0;
		int rowsFit = (outerRect.Height / rowStep).CeilToInt() + 1;

		first = Math.Max( 0, topRow * _columns );
		pastEnd = first + rowsFit * _columns;
	}

	public void Position( int index, Sandbox.UI.Panel panel )
	{
		var inner = _box.RectInner;
		inner.Position = _box.RectInner.Position - _box.Rect.Position;
		var rect = inner * _scaleFromScreen;

		int col = index % _columns;
		int row = index / _columns;
		float stepX = _cellSize.x + Spacing.x;
		float stepY = _cellSize.y + Spacing.y;

		panel.Style.Left = rect.Left + col * stepX;
		panel.Style.Top = rect.Top + row * stepY;
		panel.Style.Width = _cellSize.x;
		panel.Style.Height = _cellSize.y;
		panel.Style.Dirty();
	}

	public float GetHeight( int count )
	{
		if ( _box is null || _box.Rect.Width == 0 ) return 0f;
		var inner = _box.RectInner;
		inner.Position = _box.RectInner.Position - _box.Rect.Position;
		var rect = inner * _scaleFromScreen;
		var outerRect = _box.Rect * _scaleFromScreen;

		float rowStep = _cellSize.y + Spacing.y;
		if ( rowStep <= 0f ) return MathF.Max( 0f, outerRect.Height - rect.Height );
		float rows = MathF.Ceiling( count / (float)_columns );
		float paddingY = outerRect.Height - rect.Height;
		return rows * rowStep + MathF.Max( 0f, paddingY );
	}

	public int Columns => _columns;

	public (float top, float bottom) GetCellRowBounds( int index )
	{
		var inner = _box.RectInner;
		inner.Position = _box.RectInner.Position - _box.Rect.Position;
		var rect = inner * _scaleFromScreen;

		int row = index / (_columns < 1 ? 1 : _columns);
		float rowStep = _cellSize.y + Spacing.y;
		float cellTop = rect.Top + row * rowStep;

		return (cellTop / _scaleFromScreen, (cellTop + _cellSize.y) / _scaleFromScreen);
	}

	public float ViewportHeight( Sandbox.UI.Box box ) => box.Rect.Height;
}

public sealed class GameGrid : Sandbox.UI.Panel
{
	private readonly GameGridLayout _layout = new();
	private readonly Dictionary<int, object> _cellData = [];
	private readonly Dictionary<int, Sandbox.UI.Panel> _created = [];
	private readonly List<int> _removals = [];
	private readonly List<object> _items = [];

	private IList _sourceList;
	private int _sourceListCount;
	private bool _needsRebuild;
	private bool _lastCellCreated;
	private float _targetScrollY = float.NaN;

	public int HoveredIndex { get; set; } = -1;
	public bool UseMouseHover { get; set; } = true;
	public int Columns => _layout.Columns;

	private int _contentVersion;

	[Parameter]
	public int ContentVersion
	{
		get => _contentVersion;
		set
		{
			if ( value == _contentVersion ) return;
			_contentVersion = value;
			foreach ( var idx in _cellData.Keys.ToList() ) _cellData[idx] = null;
			_needsRebuild = true;
		}
	}

	[Parameter]
	public Vector2 ItemSize
	{
		get => new( _layout.ItemWidth, _layout.ItemHeight );
		set { _layout.ItemWidth = value.x; _layout.ItemHeight = value.y; }
	}

	[Parameter]
	public RenderFragment<object> Item { get; set; }

	[Parameter]
	public Action OnLastCell { get; set; }

	[Parameter]
	public Action<int> OnCellClick { get; set; }

	protected override void OnClick( Sandbox.UI.MousePanelEvent e )
	{
		base.OnClick( e );
		if ( HoveredIndex >= 0 )
			OnCellClick?.Invoke( HoveredIndex );
	}

	[Parameter]
	public IEnumerable<object> Items
	{
		set
		{
			if ( value is null ) { if ( _items.Count == 0 ) return; Clear(); return; }

			_sourceList = value as IList;
			List<object> mat;
			if ( _sourceList != null )
			{
				mat = [.. _sourceList];
			}
			else
			{
				mat = value.ToList();
				_sourceList = null;
			}
			_sourceListCount = mat.Count;

			if ( _items.Count == mat.Count && _items.SequenceEqual( mat ) ) return;

			_items.Clear();
			_items.AddRange( mat );
			_needsRebuild = true;
			_lastCellCreated = false;
			StateHasChanged();
		}
	}

	public GameGrid()
	{
		Style.Position = Sandbox.UI.PositionMode.Relative;
		Style.Overflow = Sandbox.UI.OverflowMode.Scroll;
	}

	public void Clear()
	{
		_items.Clear();
		_sourceList = null;
		_sourceListCount = 0;
		_needsRebuild = true;
		foreach ( var p in _created.Values ) p.Delete( true );
		_created.Clear();
		_cellData.Clear();
	}

	private void CheckSourceForChanges()
	{
		if ( _sourceList is null || _sourceList.Count == _sourceListCount ) return;
		_items.Clear();
		_sourceListCount = _sourceList.Count;
		foreach ( var x in _sourceList ) _items.Add( x );
		_needsRebuild = true;
		_lastCellCreated = false;
		StateHasChanged();
	}

	public override void Tick()
	{
		base.Tick();

		if ( ComputedStyle is null || !IsVisible ) return;

		CheckSourceForChanges();

		var colGap = ComputedStyle.ColumnGap?.Value ?? 0f;
		var rowGap = ComputedStyle.RowGap?.Value ?? 0f;
		_layout.Spacing = new Vector2( colGap, rowGap );

		bool layoutChanged = _layout.Update( Box, ScaleFromScreen, ScrollOffset.y * ScaleFromScreen );

		if ( layoutChanged || _needsRebuild )
		{
			_needsRebuild = false;
			_layout.GetVisibleRange( out int first, out int pastEnd );
			DeleteNotVisible( first, pastEnd - 1 );
			for ( int i = first; i < pastEnd; i++ )
				RefreshCreated( i );
		}

		if ( UseMouseHover )
		{
			int mouseOver = -1;
			foreach ( var (index, cell) in _created )
				if ( cell.HasHovered ) { mouseOver = index; break; }
			HoveredIndex = mouseOver;
		}

		foreach ( var (index, cell) in _created )
			cell.SetClass( "hovered", index == HoveredIndex );

		if ( !UseMouseHover && HoveredIndex >= 0 && HoveredIndex < _items.Count )
		{
			var (cellTop, cellBottom) = _layout.GetCellRowBounds( HoveredIndex );
			float viewH = _layout.ViewportHeight( Box );
			float scrollY = ScrollOffset.y;

			float padTop = (Box.RectInner.Top - Box.Rect.Top) / ScaleFromScreen;
			float padBot = (Box.Rect.Bottom - Box.RectInner.Bottom) / ScaleFromScreen;

			if ( cellTop - padTop < scrollY )
				_targetScrollY = MathF.Max( 0f, cellTop - padTop );
			else if ( cellBottom + padBot > scrollY + viewH )
				_targetScrollY = cellBottom + padBot - viewH;
		}

		if ( !float.IsNaN( _targetScrollY ) )
		{
			float current = ScrollOffset.y;
			float next = current + (_targetScrollY - current) * MathF.Min( 1f, Time.Delta * 18f );
			if ( MathF.Abs( next - _targetScrollY ) < 0.5f )
			{
				next = _targetScrollY;
				_targetScrollY = float.NaN;
			}
			ScrollOffset = new Vector2( ScrollOffset.x, next );
		}
	}

	protected override void FinalLayoutChildren( Vector2 offset )
	{
		foreach ( var kv in _created )
			kv.Value.FinalLayout( offset );

		var rect = Box.Rect;
		rect.Position -= ScrollOffset;
		rect.Height = MathF.Max( _layout.GetHeight( _items.Count ) * ScaleToScreen, rect.Height );
		ConstrainScrolling( rect.Size );
	}

	private void DeleteNotVisible( int minInclusive, int maxInclusive )
	{
		_removals.Clear();
		foreach ( var idx in _created.Keys )
			if ( idx < minInclusive || idx > maxInclusive || idx >= _items.Count )
				_removals.Add( idx );

		foreach ( var idx in _removals )
		{
			if ( _created.Remove( idx, out var p ) ) p.Delete( true );
			_cellData.Remove( idx );
		}
		_removals.Clear();
	}

	private void RefreshCreated( int i )
	{
		if ( i < 0 || i >= _items.Count ) return;

		var data = _items[i];
		bool needRebuild = !_cellData.TryGetValue( i, out var last ) || !EqualityComparer<object>.Default.Equals( last, data );

		if ( !_created.TryGetValue( i, out var panel ) || needRebuild )
		{
			panel?.Delete( true );
			panel = Add.Panel( "cell" );
			panel.Style.Position = Sandbox.UI.PositionMode.Absolute;
			panel.ChildContent = Item?.Invoke( data );
			_created[i] = panel;
			_cellData[i] = data;

			if ( _items.Count - 1 == i && !_lastCellCreated )
			{
				_lastCellCreated = true;
				OnLastCell?.InvokeWithWarning();
			}
		}

		_layout.Position( i, panel );
	}
}
