using System;
using System.Collections.Generic;

namespace OpenSyno
{
    public class IoC
    {
        private Dictionary<Type, object> _registeredInstances = new Dictionary<Type, object>();
        static private readonly IoC _current = new IoC();
        static public IoC Current
        {
            get { return _current; }            
        }

        public T Resolve<T>()
        {
            if (_registeredInstances.ContainsKey(typeof(T)))
            {
                return (T) _registeredInstances[typeof (T)];
            }

            throw new ArgumentException(string.Format("The type {0} could not be resolved as it was not previously registered.", typeof (T).FullName));
        }

        public void RegisterInstance<T>(T instance)
        {
            if (_registeredInstances.ContainsKey(typeof(T)))
            {
                _registeredInstances[typeof (T)] = instance;
            }
            else
            {
                _registeredInstances.Add(typeof(T), instance);                
            }
        }

        public bool CheckRegistered<T>()
        {
            return _registeredInstances.ContainsKey(typeof (T));
        }
    }
}