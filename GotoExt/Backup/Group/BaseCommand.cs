using System;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using GotoExt.Biz;
using Microsoft.VisualStudio.Shell;

namespace GotoExt.Command
{
    /// <summary>
    /// 命令基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class BaseCommand<T> where T : BaseBiz
    {
        #region 抽象属性

        /// <summary>
        /// Command ID.
        /// </summary>
        public abstract int CommandId { get; }

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public abstract Guid CommandSet { get; }

        #endregion

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        protected Package Package;

        /// <summary>
        /// vs自动化对象
        /// </summary>
        protected DTE2 Dte;

        /// <summary>
        /// 对应业务对象
        /// </summary>
        protected T Biz;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        protected BaseCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            Package = package;
            Dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            Biz = (T)Activator.CreateInstance(typeof(T), Dte);

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        protected IServiceProvider ServiceProvider => this.Package;

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            if (myCommand != null)
            {
                Query(myCommand);
            }
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                Call();
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        #region 虚方法

        /// <summary>
        /// 命令展示前
        /// </summary>
        /// <param name="myCommand"></param>
        protected virtual void Query(OleMenuCommand myCommand)
        {
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        protected virtual void Call()
        {
            Biz.Goto();
        }

        #endregion
    }
}
