﻿using System.Linq;
using nUnitShouldAdapter;
using NUnit.Specifications;
using paramore.brighter.commandprocessor.tests.nunit.CommandProcessors.TestDoubles;
using TinyIoC;

namespace paramore.brighter.commandprocessor.tests.nunit.CommandProcessors
{
    [Subject(typeof(PipelineBuilder<>))]
    public class When_Building_An_Async_Pipeline_Allow_Pre_And_Post_Tasks : ContextSpecification
    {
        private static PipelineBuilder<MyCommand> s_pipeline_Builder;
        private static IHandleRequestsAsync<MyCommand> s_pipeline;

        private Establish _context = () =>
        {
            var registry = new SubscriberRegistry();
            registry.RegisterAsync<MyCommand, MyPreAndPostDecoratedHandlerAsync>();

            var container = new TinyIoCContainer();
            var handlerFactory = new TinyIocHandlerFactoryAsync(container);
            container.Register<IHandleRequestsAsync<MyCommand>, MyPreAndPostDecoratedHandlerAsync>();
            container.Register<IHandleRequestsAsync<MyCommand>, MyValidationHandlerAsync<MyCommand>>();
            container.Register<IHandleRequestsAsync<MyCommand>, MyLoggingHandlerAsync<MyCommand>>();

            s_pipeline_Builder = new PipelineBuilder<MyCommand>(registry, handlerFactory);
        };

        private Because _of = () => s_pipeline = s_pipeline_Builder.BuildAsync(new RequestContext(), false).First();

        private It _should_add_handlers_in_the_correct_sequence_into_the_chain = () => TraceFilters().ToString().ShouldEqual("MyValidationHandlerAsync`1|MyPreAndPostDecoratedHandlerAsync|MyLoggingHandlerAsync`1|");

        private static PipelineTracer TraceFilters()
        {
            var pipelineTracer = new PipelineTracer();
            s_pipeline.DescribePath(pipelineTracer);
            return pipelineTracer;
        }
    }
}
