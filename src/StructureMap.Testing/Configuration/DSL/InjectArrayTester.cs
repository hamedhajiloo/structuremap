using Shouldly;
using StructureMap.Pipeline;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace StructureMap.Testing.Configuration.DSL
{
    public class InjectArrayTester
    {
        public class Processor
        {
            private readonly IHandler[] _handlers;
            private readonly string _name;

            public Processor(IHandler[] handlers, string name)
            {
                _handlers = handlers;
                _name = name;
            }

            public IHandler[] Handlers
            {
                get { return _handlers; }
            }

            public string Name
            {
                get { return _name; }
            }
        }

        public class ProcessorWithList
        {
            private readonly IList<IHandler> _handlers;
            private readonly string _name;

            public ProcessorWithList(IList<IHandler> handlers, string name)
            {
                _handlers = handlers;
                _name = name;
            }

            public IList<IHandler> Handlers
            {
                get { return _handlers; }
            }

            public string Name
            {
                get { return _name; }
            }
        }

        public class ProcessorWithConcreteList
        {
            private readonly List<IHandler> _handlers;
            private readonly string _name;

            public ProcessorWithConcreteList(List<IHandler> handlers, string name)
            {
                _handlers = handlers;
                _name = name;
            }

            public IList<IHandler> Handlers
            {
                get { return _handlers; }
            }

            public string Name
            {
                get { return _name; }
            }
        }

        public class ProcessorWithEnumerable
        {
            private readonly IList<IHandler> _handlers;
            private readonly string _name;

            public ProcessorWithEnumerable(IEnumerable<IHandler> handlers, string name)
            {
                _handlers = handlers.ToList();
                _name = name;
            }

            public IList<IHandler> Handlers
            {
                get { return _handlers; }
            }

            public string Name
            {
                get { return _name; }
            }
        }

        public class Processor2
        {
            private readonly IHandler[] _first;
            private readonly IHandler[] _second;

            public Processor2(IHandler[] first, IHandler[] second)
            {
                _first = first;
                _second = second;
            }

            public IHandler[] First
            {
                get { return _first; }
            }

            public IHandler[] Second
            {
                get { return _second; }
            }
        }

        public interface IHandler
        {
        }

        public class Handler1 : IHandler
        {
        }

        public class Handler2 : IHandler
        {
        }

        public class Handler3 : IHandler
        {
        }

        [Fact]
        public void CanStillAddOtherPropertiesAfterTheCallToChildArray()
        {
            var container = new Container(x =>
            {
                x.For<Processor>().Use<Processor>()
                    .EnumerableOf<IHandler>().Contains(
                        new SmartInstance<Handler1>(),
                        new SmartInstance<Handler2>(),
                        new SmartInstance<Handler3>()
                    )
                    .Ctor<string>("name").Is("Jeremy");
            });

            container.GetInstance<Processor>().Name.ShouldBe("Jeremy");
        }

        [Fact]
        public void get_a_configured_list()
        {
            var container = new Container(x =>
            {
                x.For<ProcessorWithList>().Use<ProcessorWithList>()
                    .EnumerableOf<IHandler>().Contains(
                        new SmartInstance<Handler1>(),
                        new SmartInstance<Handler2>(),
                        new SmartInstance<Handler3>()
                    )
                    .Ctor<string>("name").Is("Jeremy");
            });

            container.GetInstance<ProcessorWithList>()
                .Handlers.Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(Handler1), typeof(Handler2), typeof(Handler3));
        }

        [Fact]
        public void get_a_configured_concrete_list()
        {
            var container = new Container(x =>
            {
                x.For<ProcessorWithConcreteList>().Use<ProcessorWithConcreteList>()
                    .EnumerableOf<IHandler>().Contains(
                        new SmartInstance<Handler1>(),
                        new SmartInstance<Handler2>(),
                        new SmartInstance<Handler3>()
                    )
                    .Ctor<string>("name").Is("Jeremy");
            });

            container.GetInstance<ProcessorWithConcreteList>()
                .Handlers.Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(Handler1), typeof(Handler2), typeof(Handler3));
        }

        [Fact]
        public void get_a_configured_ienumerable()
        {
            var container = new Container(x =>
            {
                x.For<ProcessorWithEnumerable>().Use<ProcessorWithEnumerable>()
                    .EnumerableOf<IHandler>().Contains(
                        new SmartInstance<Handler1>(),
                        new SmartInstance<Handler2>(),
                        new SmartInstance<Handler3>()
                    )
                    .Ctor<string>("name").Is("Jeremy");
            });

            container.GetInstance<ProcessorWithEnumerable>()
                .Handlers.Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(Handler1), typeof(Handler2), typeof(Handler3));
        }

        [Fact]
        public void InjectPropertiesByName()
        {
            var container = new Container(r =>
            {
                r.For<Processor2>().Use<Processor2>()
                    .EnumerableOf<IHandler>("first").Contains(x =>
                    {
                        x.Type<Handler1>();
                        x.Type<Handler2>();
                    })
                    .EnumerableOf<IHandler>("second").Contains(x =>
                    {
                        x.Type<Handler2>();
                        x.Type<Handler3>();
                    });
            });

            var processor = container.GetInstance<Processor2>();

            processor.First[0].ShouldBeOfType<Handler1>();
            processor.First[1].ShouldBeOfType<Handler2>();
            processor.Second[0].ShouldBeOfType<Handler2>();
            processor.Second[1].ShouldBeOfType<Handler3>();
        }

        [Fact]
        public void inline_definition_of_enumerable_child_respects_order_of_registration()
        {
            IContainer container = new Container(r =>
            {
                r.For<IHandler>().Add<Handler1>().Named("One");
                r.For<IHandler>().Add<Handler2>().Named("Two");

                r.For<Processor>().Use<Processor>()
                    .Ctor<string>("name").Is("Jeremy")
                    .EnumerableOf<IHandler>().Contains(x =>
                    {
                        x.TheInstanceNamed("Two");
                        x.TheInstanceNamed("One");
                    });
            });

            var processor = container.GetInstance<Processor>();
            processor.Handlers[0].ShouldBeOfType<Handler2>();
            processor.Handlers[1].ShouldBeOfType<Handler1>();
        }

        [Fact]
        public void PlaceMemberInArrayByReference_with_SmartInstance()
        {
            IContainer manager = new Container(registry =>
            {
                registry.For<IHandler>().Add<Handler1>().Named("One");
                registry.For<IHandler>().Add<Handler2>().Named("Two");

                registry.For<Processor>().Use<Processor>()
                    .Ctor<string>("name").Is("Jeremy")
                    .EnumerableOf<IHandler>().Contains(x =>
                    {
                        x.TheInstanceNamed("Two");
                        x.TheInstanceNamed("One");
                    });
            });

            var processor = manager.GetInstance<Processor>();

            processor.Handlers[0].ShouldBeOfType<Handler2>();
            processor.Handlers[1].ShouldBeOfType<Handler1>();
        }

        [Fact]
        public void ProgrammaticallyInjectArrayAllInline()
        {
            var container = new Container(x =>
            {
                x.For<Processor>().Use<Processor>()
                    .Ctor<string>("name").Is("Jeremy")
                    .EnumerableOf<IHandler>().Contains(y =>
                    {
                        y.Type<Handler1>();
                        y.Type<Handler2>();
                        y.Type<Handler3>();
                    });
            });

            var processor = container.GetInstance<Processor>();

            processor.Handlers[0].ShouldBeOfType<Handler1>();
            processor.Handlers[1].ShouldBeOfType<Handler2>();
            processor.Handlers[2].ShouldBeOfType<Handler3>();
        }

        [Fact]
        public void ProgrammaticallyInjectArrayAllInline_with_smart_instance()
        {
            IContainer container = new Container(r =>
            {
                r.For<Processor>().Use<Processor>()
                    .Ctor<string>("name").Is("Jeremy")
                    .EnumerableOf<IHandler>().Contains(x =>
                    {
                        x.Type<Handler1>();
                        x.Type<Handler2>();
                        x.Type<Handler3>();
                    });
            });

            var processor = container.GetInstance<Processor>();

            processor.Handlers[0].ShouldBeOfType<Handler1>();
            processor.Handlers[1].ShouldBeOfType<Handler2>();
            processor.Handlers[2].ShouldBeOfType<Handler3>();
        }
    }
}