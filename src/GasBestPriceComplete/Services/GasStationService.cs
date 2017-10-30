using GasBestPrice.Model;
using Lime.Messaging.Contents;
using Lime.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Take.Blip.Client.Extensions.Bucket;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace GasBestPrice.Services
{
    public class GasStationService : IGasStationService
    {
        private readonly IBucketExtension _bucketExtension;
        private static ConcurrentDictionary<string, GasStation> _database = new ConcurrentDictionary<string, GasStation>();

        static GasStationService()
        {
            _database.TryAdd("1", new GasStation {
                Id = "1",
                Address = "Rua Mais bonita da cidade",
                AlcoholPrice = 3.89f,
                GasolinePrice = 4.81f,
                LastUpdated = DateTimeOffset.Now,
                Latitude = -19.9292403f,
                Longitude = -43.9543158f
            });
            _database.TryAdd("2", new GasStation {
                Id = "2",
                Address = "Rua das dores",
                AlcoholPrice = 2.89f,
                GasolinePrice = 3.81f,
                LastUpdated = DateTimeOffset.Now,
                Latitude = -19.9292403f,
                Longitude = -43.9543158f
            });
            _database.TryAdd("3", new GasStation {
                Id = "3",
                Address = "Rua dos bots do sucesso",
                AlcoholPrice = 2.77f,
                GasolinePrice = 3.99f,
                LastUpdated = DateTimeOffset.Now,
                Latitude = -19.9292403f,
                Longitude = -43.9543158f
            });
        }

        public GasStationService(IBucketExtension bucketExtension)
        {
            _bucketExtension = bucketExtension;
        }

        public async Task<DocumentCollection> GetNearGasStationsAsync(Location location)
        {
            //Simulating a network request
            await Task.Delay(2000);

            //Get all gas stations via API or whatever other service
            var nearGasStationsList = _database.Values.ToList();

            return GetCarouselFromGasStationList(nearGasStationsList, false);
        }

        public async Task<DocumentCollection> GetFavoritesGasStationsAsync(Identity userIdentity)
        {
            var myContextKey = $"{userIdentity}:context";

            var myContext = await _bucketExtension.GetAsync<JsonDocument>(myContextKey);
            var favoriteIds = myContext.ContainsKey("favoriteIds") ? (myContext["favoriteIds"] as JArray).ToObject<List<string>>() : new List<string>();

            var favoriteList = new List<GasStation>();

            foreach (var id in favoriteIds)
            {
                favoriteList.Add(_database[id]);
            }

            return GetCarouselFromGasStationList(favoriteList, true);
        }

        public async Task<GasStation> GetAsync(string gasStationId)
        {
            //Simulating a network request
            await Task.Delay(2000);
            //Get a gas stations via API or whatever other service
            return _database[gasStationId];
        }

        private DocumentCollection GetCarouselFromGasStationList(List<GasStation> gasStationList, bool isFavorite = false)
        {
            if (gasStationList.Count == 0) return null;

            var count = gasStationList.Count < 4 ? gasStationList.Count : 4;

            var carouselDocument = new DocumentCollection
            {
                ItemType = DocumentSelect.MediaType,
                Items = new DocumentSelect[count],
            };

            for (int i = 0; i < count; i++)
            {
                var gasStation = gasStationList[i];

                carouselDocument.Items[i] =
                    new DocumentSelect
                    {
                        Header = new DocumentContainer
                        {
                            Value = new MediaLink
                            {
                                Uri = new Uri("http://www.botsbrasil.com.br/wp-content/uploads/2016/12/take.png"),
                                Type = new MediaType("image", "png"),
                                Title = $"Gasolina {gasStation.GasolinePrice} | Álcool {gasStation.AlcoholPrice}",
                                Text = gasStation.Address
                            }
                        },
                        Options = GetCarouselOptions(gasStation.Id, isFavorite)
                    };
            }

            return carouselDocument;
        }

        private DocumentSelectOption[] GetCarouselOptions(string gasStationId, bool isFavorite)
        {
            var fixPriceOption = new DocumentSelectOption
            {
                Label = new DocumentContainer { Value = "✍️ Corrigir preço" },
                Value = new DocumentContainer { Value = new Trigger { StateId = "3.1.1", Payload = gasStationId } },
            };

            var favoriteGastationOption =
                isFavorite ?
                new DocumentSelectOption
                {
                    Label = new DocumentContainer { Value = "🚫 Desfavoritar" },
                    Value = new DocumentContainer { Value = new Trigger { StateId = "3.1.2", Payload = gasStationId } },
                }
                :
                new DocumentSelectOption
                {
                    Label = new DocumentContainer { Value = "⭐ Favoritar posto" },
                    Value = new DocumentContainer { Value = new Trigger { StateId = "3.1.2", Payload = gasStationId } },
                };

            var routeOption = new DocumentSelectOption
            {
                Label = new DocumentContainer { Value = "🚗 Definir rota" },
                Value = new DocumentContainer { Value = new Trigger { StateId = "3.1.3", Payload = gasStationId } },
            };

            return new DocumentSelectOption[]
            {
                fixPriceOption,
                favoriteGastationOption,
                routeOption
            };
        }

        public async Task FavoriteGasStationAsync(Identity userIdentity, string gasStationId)
        {
            var myContextKey = $"{userIdentity}:context";
            var contextDocument = await _bucketExtension.GetAsync<JsonDocument>(myContextKey);

            var favoriteIds = contextDocument.ContainsKey("favoriteIds") ? (contextDocument["favoriteIds"] as JArray).ToObject<List<string>>() : new List<string>();

            if (!favoriteIds.Contains(gasStationId))
            {
                favoriteIds.Add(gasStationId);
                contextDocument["favoriteIds"] = favoriteIds;
                await _bucketExtension.SetAsync(myContextKey, contextDocument);
            }
        }

        public async Task UpdateGasolinePriceAsync(string gasStationId, float gasolinePrice)
        {
            var gasStation = await GetAsync(gasStationId);

            gasStation.GasolinePrice = gasolinePrice;
            Update(gasStation);
        }

        public async Task UpdateAlcoholPriceAsync(string gasStationId, float alcoholPrice)
        {
            var gasStation = await GetAsync(gasStationId);

            gasStation.AlcoholPrice = alcoholPrice;
            Update(gasStation);
        }

        public void Update(GasStation gasStation)
        {
            _database.AddOrUpdate(gasStation.Id, gasStation, (id, g) => g);
        }
    }

    public interface IGasStationService
    {
        Task<DocumentCollection> GetNearGasStationsAsync(Location location);

        Task<DocumentCollection> GetFavoritesGasStationsAsync(Identity userIdentity);

        Task<GasStation> GetAsync(string gasStationId);
        Task FavoriteGasStationAsync(Identity userIdentity, string gasStationId);
        Task UpdateGasolinePriceAsync(string gasStationId, float gasolinePrice);
        Task UpdateAlcoholPriceAsync(string gasStationId, float alcoholPrice);
    }
}
