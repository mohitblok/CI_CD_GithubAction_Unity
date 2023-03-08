using System;
using UnityEngine;

namespace Logging
{
    public interface ICheckNetworkStability
    {
        double Ping { get; }

        void State();
        Type NetType();
    }

    public class NetworkStabilityLogger : GameLogger, ICheckNetworkStability
    {

        public NetworkStabilityLogger(string name, ILogHandler logHandler, ICheckNetworkStability wrap) : base(name, logHandler)
        {
            wrapped = wrap;
        }

        [SerializeReference] private ICheckNetworkStability wrapped;

        public double Ping
        {
            get
            {
                Log("Ping retrieved");
                return wrapped.Ping;
            }
        }

        public void State()
        {
            wrapped.State();
            Log("State retrieved");
        }

        public Type NetType()
        {
            Log("NetType retrieved");
            return wrapped.NetType();
        }
    }

    public static class NetworkStabilityLocator
    {
        public static ICheckNetworkStability Service { get; private set; }

        public static void Provide(ICheckNetworkStability service)
        {
            Service = service;
        }
    }
}