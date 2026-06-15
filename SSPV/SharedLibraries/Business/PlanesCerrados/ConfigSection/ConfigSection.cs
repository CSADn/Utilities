using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.Configuration
{
    public class ConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        [ConfigurationCollection(typeof(ConfigCollection), AddItemName = "add")]
        public ConfigCollection Instances
        {
            get { return (ConfigCollection)this[""]; }
            set { this[""] = value; }
        }
    }

    public class ConfigCollection : ConfigurationElementCollection, IEnumerable<ConfigInstance>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConfigInstance();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConfigInstance)element).Name;
        }

        public ConfigInstance this[int index]
        {
            get { return (ConfigInstance)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Remove(string name)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        IEnumerator<ConfigInstance> IEnumerable<ConfigInstance>.GetEnumerator()
        {
            foreach (var key in BaseGetAllKeys())
                yield return (ConfigInstance)BaseGet(key);
        }
    }

    public class ConfigInstance : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name { get { return (string)this["name"]; } set { this["name"] = value;  } }

        [ConfigurationProperty("endpoint", IsRequired = true)]
        public string Endpoint { get { return (string)this["endpoint"]; } set { this["endpoint"] = value; } }

        [ConfigurationProperty("appUser", IsRequired = true)]
        public string AppUser { get { return (string)this["appUser"]; } set { this["appUser"] = value; } }

        [ConfigurationProperty("appKey", IsRequired = true)]
        public string AppKey { get { return (string)this["appKey"]; } set { this["appKey"] = value; } }
    }
}
