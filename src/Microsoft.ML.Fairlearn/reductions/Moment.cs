﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Analysis;

namespace Microsoft.ML.Fairlearn.reductions
{
    /// <summary>
    /// General Moment of :class:`Moment` objects to describe the disparity constraints imposed
    /// on the solution.This is an abstract class for all such objects.
    /// </summary>
    public abstract class Moment
    {
        private bool _dataLoaded = false;
        protected DataFrameColumn Y; //maybe lowercase this?
        public DataFrame Tags { get; private set; }
        public IDataView X { get; protected set; }
        public long TotalSamples { get; protected set; }

        public DataFrameColumn SensitiveFeatureColumn { get => Tags["group_id"]; }

        public Moment()
        {

        }
        public void LoadData(IDataView x, DataFrameColumn y, StringDataFrameColumn sensitiveFeature = null)
        {
            if (_dataLoaded)
            {
                //throw new InvalidOperationException("data can be loaded only once");
            }

            X = x;
            TotalSamples = y.Length;
            Y = y;
            Tags = new DataFrame();
            Tags["label"] = y;

            if (sensitiveFeature != null)
            {
                // _tags["group_id"] = DataFrameColumn.Create; maybe convert from a vector?
                Tags["group_id"] = sensitiveFeature;
            }
            _dataLoaded = true;
        }

        public abstract DataFrame Gamma(PrimitiveDataFrameColumn<float> yPred);
        public float Bound()
        {
            throw new NotImplementedException();
        }
        public float ProjectLambda()
        {
            throw new NotImplementedException();
        }
        public virtual DataFrameColumn SignedWeights(DataFrame lambdaVec)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Moment that can be expressed as weighted classification error.
    /// </summary>
    public abstract class ClassificationMoment : Moment
    {

    }
}
