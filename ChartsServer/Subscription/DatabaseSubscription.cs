using ChartsServer.Hubs;
using ChartsServer.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using TableDependency.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChartsServer.Subscription
{
    public interface IDatabaseSubscription
    {
        void Configure(string tableName);
    }
    public class DatabaseSubscription<T> : IDatabaseSubscription where T:class, new()
    {
        IConfiguration _configuration;
        IHubContext<SatisHub> _hubContext;
        public DatabaseSubscription(IConfiguration configuration, IHubContext<SatisHub> hubContext)
        {
            _configuration = configuration;
            _hubContext = hubContext;
        }
        SqlTableDependency<T> _tableDependency;
        public void Configure(string tableName)
        {
            _tableDependency = new SqlTableDependency<T>(_configuration.GetConnectionString("SQL"),tableName);
            _tableDependency.OnChanged += async (o, e) =>
            {
                SatisDBContext context = new SatisDBContext();

                var data = (from personel in context.Personellers
                             join satis in context.Satislars
                             on personel.Id equals satis.PersonelId
                             select new { personel, satis }).ToList() ;
                List<object> datas = new List<object>();
                var personelIsımleri = data.Select(data => data.personel.Adi).Distinct().ToList();
                personelIsımleri.ForEach(p =>
                {
                    datas.Add(new
                    {
                        PersonelAdi = p,
                        Satislar = data.Where(s => s.personel.Adi == p).Select(s => s.satis.Fiyat).ToList()
                    });
                        
                });
                await _hubContext.Clients.All.SendAsync("receiveMessage", datas);

            };
            _tableDependency.OnError += (o, e) =>
            {

            };
            _tableDependency.Start();
        }

        ~DatabaseSubscription()
        {
            _tableDependency.Stop();
        }
    }
}
