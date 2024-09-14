using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace apiBinance.Bussines
{
    public class MarketBussines : IDisposable
    {
        private readonly IBinanceSocketClient _webSocketClient;
        private UpdateSubscription _subscription;

        public MarketBussines(IBinanceSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient;
        }

        public async Task Start(string symbol)
        {
            int c = 0;
            // Manejar suscripciones con control de errores
            var result = await _webSocketClient.UsdFuturesApi.SubscribeToKlineUpdatesAsync(symbol, KlineInterval.OneMinute, async data =>
            {
                // Proceso de datos de mercado (precios, etc.)
                Console.WriteLine("Precio de cierre: " + data.Data.Data.ClosePrice + " Cierra vela: " + data.Data.Data.Final.ToString());
                //borrar
                c++;
                if (c == 6)
                {
                    c = 0;
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                }
                //borrar
            });

            if (!result.Success)
            {
                // error de conexión
                if (result.Error is SocketException || result.Error?.Message.Contains("Network", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new SocketException(); // Disparar reconexión si es un error de red
                }
                else
                {
                    throw new Exception($"Error al suscribirse: {result.Error?.Message}");
                }
            }

            _subscription = result.Data;
            Console.WriteLine($"Suscripción exitosa a {symbol}");
        }

        public void Stop()
        {
            // Desuscribir si existe una suscripción activa
            if (_subscription != null)
            {
                _webSocketClient.UnsubscribeAsync(_subscription);
            }
        }

        public void Dispose()
        {
            // Detener el WebSocket y limpiar recursos
            Stop();
            _webSocketClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}

