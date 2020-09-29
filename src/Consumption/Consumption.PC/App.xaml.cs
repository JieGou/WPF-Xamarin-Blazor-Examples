﻿using Consumption.Service;
using Consumption.ViewModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using Consumption.ViewModel.Common;
using Autofac;
using System.Reflection;
using Consumption.Core.Common;
using Consumption.ViewModel.Core;
using Consumption.ViewModel.Interfaces;
using GalaSoft.MvvmLight;
using Consumption.Shared.DataInterfaces;
using Consumption.Shared.Common;

namespace Consumption.PC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
            NetCoreProvider.Get<ILog>()?.Warn(e.Exception, e.Exception.Message);
            e.Handled = true;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            Contract.serverUrl = ConfigurationManager.AppSettings["serverAddress"];
            var container = ConfigureServices();
            NetCoreProvider.RegisterServiceLocator(container);
            var login = NetCoreProvider.Get<IModuleDialog>("LoginCenter");
            await login.ShowDialog();
        }

        private IContainer ConfigureServices()
        {
            //创建依赖容器
            ContainerBuilder builder = new ContainerBuilder();
            //注册日志服务
            builder.RegisterType<ConsumptionNLog>().As<ILog>().SingleInstance();
            //注册HTTP服务依赖关系
            builder.AddCustomRepository<UserService, IUserRepository>()
                .AddCustomRepository<GroupService, IGroupRepository>()
                .AddCustomRepository<MenuService, IMenuRepository>()
                .AddCustomRepository<BasicService, IBasicRepository>();
            //注册ViewModel依赖关系
            builder.AddCustomViewModel<UserViewModel, IUserViewModel>()
                .AddCustomViewModel<GroupViewModel, IGroupViewModel>()
                .AddCustomViewModel<MenuViewModel, IMenuViewModel>()
                .AddCustomViewModel<BasicViewModel, IBasicViewModel>();

            //注册程序集
            var assembly = Assembly.GetCallingAssembly();
            var types = assembly.GetTypes();
            foreach (var t in types)
            {
                //简陋判断一下,一般而言,该定义仅仅注册Center结尾的类依赖关系。
                if (t.Name.EndsWith("Center"))
                {
                    var firstInterface = t.GetInterfaces().FirstOrDefault();
                    if (firstInterface != null)
                        builder.RegisterType(t).Named(t.Name, firstInterface);
                }
            }
            return builder.Build();
        }

    }
}
