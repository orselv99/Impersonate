using Impersonate.Util;
using System.ServiceProcess;

namespace Impersonate
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();

            // pipe 서버 생성
            var server = new PipeServerUtil(ProcessUtil.Start, "impersonate_test");
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
