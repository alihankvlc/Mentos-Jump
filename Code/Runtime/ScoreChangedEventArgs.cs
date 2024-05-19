using System;
using System.Collections;
using UnityEngine;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    public class ScoreChangedEventArgs<T> : EventArgs where T : struct
    {
        public T GetValue;

        public ScoreChangedEventArgs(T param)
        {
            GetValue = param;
        }
    }
}