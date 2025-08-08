namespace GameLogic
{
    public class CharacterInputLayer : IInputLayer
    {
        public int Priority => 0;

        public void Clear()
        {
        }

        /// <summary>
        /// Layer中有两种输入触发方式，一种是直接触发事件，另一种是将输入输出到蓝图事件中，通过蓝图里对输入的各种Tag判断，状态判断，最终得出返回的指令，或是3C的跳跃，或是返回一个技能ID
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public SInputCommand HandleHold()
        {
            throw new System.NotImplementedException();
        }

        public void HandleHoldEvent()
        {
            throw new System.NotImplementedException();
        }

        public SInputCommand HandlePress()
        {
            throw new System.NotImplementedException();
        }

        public void HandlePressEvent()
        {
            throw new System.NotImplementedException();
        }

        public SInputCommand HandleRelease()
        {
            throw new System.NotImplementedException();
        }

        public void HandleReleaseEvent()
        {
            throw new System.NotImplementedException();
        }
    }
}