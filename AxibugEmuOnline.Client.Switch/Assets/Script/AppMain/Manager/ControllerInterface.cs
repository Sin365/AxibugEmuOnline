/// <summary>
/// 负责管理本地控制器与具体游戏之间的槽位分配
/// </summary>
public interface IControllerSetuper
{
    /// <summary>
    /// 设置本地手柄与游戏手柄槽位的映射,这个方法是一个全量更新手柄插入设置的方法
    /// </summary>
    void SetConnect(
        uint? con0ToSlot = null,
        uint? con1ToSlot = null,
        uint? con2ToSlot = null,
        uint? con3ToSlot = null);

    /// <summary>
    /// 增量式的修改一个手柄和一个槽位的连接关系
    /// </summary>
    /// <param name="conIndex"></param>
    /// <param name="slotIndex"></param>
    void LetControllerConnect(int conIndex, uint slotIndex);

    /// <summary>
    /// 指定手柄插槽位,获取当前槽位连接的本地手柄序号
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    int? GetSlotConnectingControllerIndex(int slotIndex);
    IController GetSlotConnectingController(int slotIndex);

    /// <summary>
    /// 获得一个空的槽位
    /// </summary>
    /// <returns></returns>
    uint? GetFreeSlotIndex();


}
public interface IController
{
    bool AnyButtonDown();
}