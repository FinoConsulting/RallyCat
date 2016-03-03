using System;
using System.Collections.Generic;
using System.Threading;
using FluentData;
using RallyCat.Core.Configuration;
using RallyCat.Core.DataAccess;
using RallyCat.Core.Rally;


namespace RallyCat.Core.Services
{
    public class RallyBackgroundDataService
    {
        private static RallyBackgroundDataService _Instance;
        private static IDbContext                 _DbContext;
        private static readonly Object            _Lock = new Object();

        public RallyGlobalConfiguration RallyGlobalConfiguration { get; private set; }

        public List<RallySlackMapping> RallySlackMappings { get; private set; }

        public static DateTime LastUpdatedTime { get; private set; }

        public static RallyBackgroundDataService Instance
        {
            get
            {
                if (_Instance != null) { return _Instance; }

                lock(_Lock)
                {
                    _Instance = new RallyBackgroundDataService(_DbContext);
                    _Instance.LoadAll();
                }
                ThreadPool.QueueUserWorkItem(AutoRefresh);

                return _Instance;
            }
        }

        private RallyBackgroundDataService(IDbContext context) { SetDbContext(context); }

        public static void SetDbContext(IDbContext context) { _DbContext = context; }

        public void ForceRefresh()
        {
            lock(_Lock)
            {
                _Instance = new RallyBackgroundDataService(_DbContext);
                _Instance.LoadAll();
            }
        }

        public static void AutoRefresh(Object stateInfo)
        {
            while (true)
            {
                lock(_Lock)
                {
                    if (DateTime.Now - LastUpdatedTime >= TimeSpan.FromSeconds(20))
                    {
                        var newDataService = new RallyBackgroundDataService(_DbContext);
                        newDataService.LoadAll();

                        lock(_Lock)
                        {
                            _Instance = newDataService;
                        }
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(20));
            }
        }

        private void LoadAll()
        {
            RallyGlobalConfiguration = GetRallyGlobalConfiguration();
            RallySlackMappings       = GetRallySlackMappings();
            LastUpdatedTime = DateTime.Now;
        }

        private static RallyGlobalConfiguration GetRallyGlobalConfiguration()
        {
            if (_DbContext == null) { throw new ArgumentNullException("DBContext is null."); }

            var repo   = new RallyGlobalConfigurationRepository(_DbContext);
            var result = repo.GetItem();
            return result.Object;
        }

        private static List<RallySlackMapping> GetRallySlackMappings()
        {
            if (_DbContext == null) { throw new ArgumentNullException("DBContext is null."); }

            var repo   = new RallySlackMappingRepository(_DbContext);
            var result = repo.GetAll();
            return result.Object;
        }
    }
}