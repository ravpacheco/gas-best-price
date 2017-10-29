using GasBestPrice.Model;
using Lime.Messaging.Contents;
using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Take.Blip.Client.Extensions.Bucket;

namespace GasBestPrice.Services
{
    public class GasStationService : IGasStationService
    {
        private readonly IBucketExtension _bucketExtension;

        public GasStationService(IBucketExtension bucketExtension)
        {
            _bucketExtension = bucketExtension;
        }

        public async Task<DocumentCollection> GetNearGasStationsAsync(Location location)
        {
            //Simulating a network request
            await Task.Delay(5000);
            
            //Get all gas stations via API or whatever other service
            var favoriteList = new List<GasStation>
            {
                new GasStation
                {
                    Id = Guid.NewGuid().ToString(),
                    Address = "Rua Mais bonita da cidade",
                    AlcoholPrice = 2.89f,
                    GasolinePrice = 3.81f,
                    LastUpdated = DateTimeOffset.Now
                },
                new GasStation
                {
                    Id = Guid.NewGuid().ToString(),
                    Address = "Rua Mais bonita da cidade",
                    AlcoholPrice = 2.89f,
                    GasolinePrice = 3.81f,
                    LastUpdated = DateTimeOffset.Now
                },
            };

            return GetCarouselFromGasStationList(favoriteList, false);
        }

        public async Task<DocumentCollection> GetFavoritesGasStationsAsync(Identity userIdentity)
        {
            var myFavoriteKey = $"{userIdentity}:myfavorites";

            var favoritesGasStationsIds = await _bucketExtension.GetAsync<JsonDocument>(myFavoriteKey);

            //foreach (var item in collection)

            var favoriteList = new List<GasStation>
            {
                new GasStation
                {
                    Id = Guid.NewGuid().ToString(),
                    Address = "Rua Mais bonita da cidade",
                    AlcoholPrice = 2.89f,
                    GasolinePrice = 3.81f,
                    LastUpdated = DateTimeOffset.Now
                }
            };

            return GetCarouselFromGasStationList(favoriteList, true);
        }

        public async Task<GasStation> GetAsync(string gasStationId)
        {
            //Simulating a network request
            await Task.Delay(5000);
            //Get a gas stations via API or whatever other service
            return new GasStation
            {
                Id = Guid.NewGuid().ToString(),
                Address = "Rua Mais bonita da cidade",
                AlcoholPrice = 2.89f,
                GasolinePrice = 3.81f,
                LastUpdated = DateTimeOffset.Now
            };
        }

        private DocumentCollection GetCarouselFromGasStationList(List<GasStation> gasStationList, bool isFavorite = false)
        {
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

            if(contextDocument == null)
            {
                contextDocument["favoritesIds"] = new List<string>() { gasStationId };
            }
            else
            {
                var idsArray = (List<string>) contextDocument["favoritesIds"];
                idsArray.Add(gasStationId);
                contextDocument["favoritesIds"] = idsArray;
            }

            await _bucketExtension.SetAsync(myContextKey, contextDocument);
        }

        public async Task UpdateGasolinePriceAsync(string gasStationId, float gasolinePrice)
        {
            var gasStation = await GetAsync(gasStationId);

            gasStation.GasolinePrice = gasolinePrice;
            await UpdateAsync(gasStation);
        }

        public async Task UpdateAlcoholPriceAsync(string gasStationId, float alcoholPrice)
        {
            var gasStation = await GetAsync(gasStationId);

            gasStation.AlcoholPrice = alcoholPrice;
            await UpdateAsync(gasStation);
        }

        public async Task<GasStation> UpdateAsync(GasStation gasStation)
        {
            return null;
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
