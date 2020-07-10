using Akka.Actor;
using Akka.Configuration.Hocon;
using System;
using System.Configuration;
using System.Reflection.Metadata.Ecma335;

namespace Akka.Persistence.Simple
{
    class Program
    {
        public class Add { }

        public class Get { }

        public class PersistEvent
        {
            public int count = 0;
            public PersistEvent (int n)
            {
                this.count = n;
            }
        }

        public class MyActor : ReceivePersistentActor
        {
            private int state = 0;

            public override string PersistenceId => "myActor";

            public MyActor() 
            {
                Recover<PersistEvent>(e =>
                {
                    this.state = e.count;
                });

                Command<Add>(_ =>
                {
                    state++;
                    Persist(new PersistEvent(state), _ => Console.WriteLine($"State {state} persist."));
                } );

                Command<Get>(_ => Sender.Tell(state));
            }
        }
        static void Main(string[] args)
        {
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var clusterConfig = section.AkkaConfig;
            var system = ActorSystem.Create("PersistAsync", clusterConfig);
            var persistentActor = system.ActorOf<MyActor>();

            for (var i = 0; i < 10; i++)
            {
                persistentActor.Tell(new Add());
            }

            var state = persistentActor.Ask(new Get()).Result;
            Console.WriteLine($"Get state: {state}");

            Console.ReadKey();
        }
    }
}
