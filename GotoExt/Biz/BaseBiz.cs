using EnvDTE80;

namespace GotoExt.Biz
{
    public interface IBaseBiz
    {
        void Goto();
    }

    /// <summary>
    /// Goto业务基类
    /// </summary>
    public abstract class BaseBiz : IBaseBiz
    {
        protected readonly DTE2 Dte;

        protected BaseBiz(DTE2 dte)
        {
            Dte = dte;
        }

        public abstract void Goto();
    }
}
