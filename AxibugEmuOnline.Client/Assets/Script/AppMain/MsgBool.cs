/// <summary>
/// String��Bool�ķ�Ϲ�
/// </summary>
public struct MsgBool
{
    public string ErrorMsg;
    public bool Value;

    public override readonly string ToString()
    {
        if (Value)
        {
            return true.ToString();
        }
        else
        {
            return ErrorMsg;
        }
    }

    public static implicit operator MsgBool(string errorMsg)
    {
        return new MsgBool { Value = false, ErrorMsg = errorMsg };
    }

    public static implicit operator MsgBool(bool value)
    {
        return new MsgBool { Value = value };
    }

    public static implicit operator bool(MsgBool msgBool)
    {
        return msgBool.Value;
    }

    public static implicit operator (bool, string)(MsgBool msgBool)
    {
        return (msgBool.Value, msgBool.ErrorMsg);
    }

    public static implicit operator string(MsgBool msgBool)
    {
        return msgBool.ToString();
    }
}
