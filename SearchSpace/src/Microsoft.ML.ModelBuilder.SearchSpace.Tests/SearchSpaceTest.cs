﻿// <copyright file="SearchSpaceTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using FluentAssertions;
using Microsoft.ML.ModelBuilder.SearchSpace.Option;
using Microsoft.ML.ModelBuilder.SearchSpace.Tuner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.ML.ModelBuilder.SearchSpace.Tests
{
    public class SearchSpaceTest : TestBase
    {
        public SearchSpaceTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void SearchSpace_sample_from_feature_space_test()
        {
            var ss = new SearchSpace<BasicSearchSpace>();
            var param = ss.SampleFromFeatureSpace(new[] { 0.0, 0, 0, 0 });

            param.ChoiceStr.Should().Be("a");
            param.UniformDouble.Should().Be(-1000);
            param.UniformFloat.Should().Be(-1000);
            param.UniformInt.Should().Be(-1000);

            param = ss.SampleFromFeatureSpace(new[] { 0.5, 0.5, 0.5, 0.5 });
            param.ChoiceStr.Should().Be("c");
            param.UniformDouble.Should().Be(0);
            param.UniformFloat.Should().Be(0);
            param.UniformInt.Should().Be(0);
        }

        [Fact]
        public void SearchSpace_mapping_to_feature_space_test()
        {
            var ss = new SearchSpace<BasicSearchSpace>();
            var param = ss.SampleFromFeatureSpace(new[] { 0.0, 0, 0, 0 });
            var features = ss.MappingToFeatureSpace(param);
            features.Should().BeEquivalentTo(0, 0, 0, 0);

            param = ss.SampleFromFeatureSpace(new[] { 0.5, 0.5, 0.5, 0.5 });
            features = ss.MappingToFeatureSpace(param);
            features.Should().BeEquivalentTo(0.5, 0.5, 0.5, 0.5);
        }

        [Fact]
        public void Nest_search_space_mapping_to_feature_space_test()
        {
            var ss = new SearchSpace<NestSearchSpace>();
            var param = ss.SampleFromFeatureSpace(new[] { 0.0, 0, 0, 0, 0, 0 });
            var features = ss.MappingToFeatureSpace(param);
            features.Should().BeEquivalentTo(0, 0, 0, 0, 0, 0);

            param = ss.SampleFromFeatureSpace(new[] { 0.5, 0.5, 0.5, 0.5, 0.5, 0.5 });
            features = ss.MappingToFeatureSpace(param);
            features.Should().BeEquivalentTo(0.5, 0.5, 0.5, 0.5, 0.5, 0.5);
        }

        [Fact]
        public void Nest_searchSpace_sample_from_feature_space_test()
        {
            var option = new NestSearchSpace()
            {
                BasicSS = new BasicSearchSpace()
                {
                    DefaultSearchSpace = new DefaultSearchSpace()
                    {
                        Strings = new[] { "B", "C", "D" },
                    },
                },
            };
            var ss = new SearchSpace<NestSearchSpace>(option);

            ss.FeatureSpaceDim.Should().Be(6);
            var param = ss.SampleFromFeatureSpace(new[] { 0.0, 0, 0, 0, 0, 0 });

            param.UniformDouble.Should().Be(-1000);
            param.UniformFloat.Should().Be(-1000);
            param.BasicSS.UniformInt.Should().Be(-1000);
            param.BasicSS.UniformDouble.Should().Be(-1000);
            param.BasicSS.UniformFloat.Should().Be(-1000);
            param.BasicSS.ChoiceStr.Should().Be("a");
            param.BasicSS.DefaultSearchSpace.Strings.Should().BeEquivalentTo("B", "C", "D");

            param = ss.SampleFromFeatureSpace(new[] { 0.5, 0.5, 0.5, 0.5, 0.5, 0.5});

            param.UniformDouble.Should().Be(0);
            param.UniformFloat.Should().Be(0);
            param.BasicSS.UniformInt.Should().Be(0);
            param.BasicSS.UniformDouble.Should().Be(0);
            param.BasicSS.UniformFloat.Should().Be(0);
            param.BasicSS.ChoiceStr.Should().Be("c");
            param.BasicSS.DefaultSearchSpace.Strings.Should().BeEquivalentTo("B", "C", "D");
        }

        [Fact]
        public void Search_space_add_option_test()
        {
            var ss = new SearchSpace();
            ss.FeatureSpaceDim.Should().Be(0);

            ss.Add("A", new UniformIntOption(-1000, 1000));
            ss.FeatureSpaceDim.Should().Be(1);

            var param = ss.SampleFromFeatureSpace(new[] { 0.5 });
            param["A"].AsType<int>().Should().Be(0);
        }

        [Fact]
        public void Search_space_remove_option_test()
        {
            var option = new BasicSearchSpace();
            var ss = new SearchSpace<BasicSearchSpace>(option);
            ss.FeatureSpaceDim.Should().Be(4);

            ss.Remove("UniformInt").Should().BeTrue();
            ss.FeatureSpaceDim.Should().Be(3);
            ss.Keys.Should().BeEquivalentTo("ChoiceStr", "UniformDouble", "UniformFloat");
            
            ss.SampleFromFeatureSpace(new double[] { 0, 0, 0})
                .DefaultSearchSpace.Strings.Should().BeEquivalentTo("A", "B", "C");

            ss.SampleFromFeatureSpace(new double[] { 0, 0, 0 })
                .DefaultSearchSpace.String.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Search_space_default_value_test()
        {
            var ss = new SearchSpace<NestSearchSpace>();
            var defaultTuner = new DefaultValueTuner(ss);
            var param = defaultTuner.Propose().AsType<NestSearchSpace>();

            param.UniformDouble.Should().Be(0);
            param.UniformFloat.Should().Be(0);
            param.BasicSS.UniformInt.Should().Be(0);
            param.BasicSS.UniformDouble.Should().Be(0);
            param.BasicSS.UniformFloat.Should().Be(0);
            param.BasicSS.ChoiceStr.Should().Be("a");
        }

        [Fact]
        public void Search_space_default_search_space_test()
        {
            var defaultSearchSpace = new DefaultSearchSpace()
            {
                String = "String",
                Int = 10,
                Bool = true,
                JTokenType = JTokenType.Null,
            };

            var ss = new SearchSpace<DefaultSearchSpace>(defaultSearchSpace);
            var param = ss.SampleFromFeatureSpace(new double[0]);

            param.Int.Should().Be(10);
            param.Float.Should().Be(0f);
            param.Double.Should().Be(0);
            param.Bool.Should().BeTrue();
            param.String.Should().Be("String");
            param.Strings.Should().BeEquivalentTo("A", "B", "C");
            param.JTokenType.Should().Be(JTokenType.Null);
            param.NullString.Should().BeNull();
            ss.FeatureSpaceDim.Should().Be(0);
            ss.MappingToFeatureSpace(param).Should().HaveCount(0);
        }

        private class DefaultSearchSpace
        {
            public int Int { get; set; }

            public float Float { get; set; }

            public double Double { get; set; }

            public bool Bool { get; set; }

            public string String { get; set; }

            public string[] Strings { get; set; } = new[] { "A", "B", "C" };

            public JTokenType JTokenType { get; set; }

            public string NullString { get; set; }
        }

        private class BasicSearchSpace
        {
            [Range(-1000, 1000, init: 0)]
            public int UniformInt { get; set; }

            [Choice("a", "b", "c", "d")]
            public string ChoiceStr { get; set; }

            [Range(-1000.0, 1000, init: 0)]
            public double UniformDouble { get; set; }

            [Range(-1000.0f, 1000, init: 0)]
            public float UniformFloat { get; set; }

            public DefaultSearchSpace DefaultSearchSpace { get; set; } = new DefaultSearchSpace();
        }

        private class NestSearchSpace
        {
            [Option]
            public BasicSearchSpace BasicSS { get; set; }

            [Range(-1000.0, 1000, init: 0)]
            public double UniformDouble { get; set; }

            [Range(-1000.0f, 1000, init: 0)]
            public float UniformFloat { get; set; }
        }
    }
}
