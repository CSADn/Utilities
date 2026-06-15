using Microsoft.Win32;

namespace OnvifJoystickGui
{
    /// <summary>
    /// Wrapper simplificado para trabajar con el registro de Windows
    /// </summary>
    public class RegistryHelper : IDisposable
    {
        private readonly RegistryKey _rootKey;
        private readonly string _basePath;
        private RegistryKey? _subKey;
        private bool _disposed = false;

        /// <summary>
        /// Constructor para inicializar el helper con una ruta base
        /// </summary>
        /// <param name="rootKey">Raíz del registro (ej: Registry.CurrentUser)</param>
        /// <param name="basePath">Ruta base (ej: "Software\\MiAplicacion")</param>
        public RegistryHelper(RegistryKey rootKey, string basePath)
        {
            _rootKey = rootKey ?? throw new ArgumentNullException(nameof(rootKey));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            try
            {
                // Intentamos abrir la subclave en modo escritura; si no existe, la creamos.
                _subKey = _rootKey.OpenSubKey(_basePath, true) ?? _rootKey.CreateSubKey(_basePath);
            }
            catch
            {
                _subKey = null;
            }
        }

        /// <summary>
        /// Helper para HKEY_CURRENT_USER
        /// </summary>
        public static RegistryHelper CurrentUser(string basePath) 
            => new RegistryHelper(Registry.CurrentUser, basePath);

        /// <summary>
        /// Helper para HKEY_LOCAL_MACHINE
        /// </summary>
        public static RegistryHelper LocalMachine(string basePath) 
            => new RegistryHelper(Registry.LocalMachine, basePath);

        /// <summary>
        /// Lee un valor del registro
        /// </summary>
        public T? GetValue<T>(string valueName, T? defaultValue = default)
        {
            try
            {
                var key = _subKey ?? _rootKey.OpenSubKey(_basePath);
                if (key == null) return defaultValue;

                var value = key.GetValue(valueName, defaultValue);
                return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Lee un valor string del registro
        /// </summary>
        public string? GetString(string valueName, string? defaultValue = null)
        {
            try
            {
                var key = _subKey ?? _rootKey.OpenSubKey(_basePath);
                return key?.GetValue(valueName, defaultValue) as string ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Lee un valor int del registro
        /// </summary>
        public int GetInt(string valueName, int defaultValue = 0)
        {
            try
            {
                var key = _subKey ?? _rootKey.OpenSubKey(_basePath);
                if (key == null) return defaultValue;

                var value = key.GetValue(valueName, defaultValue);
                return value is int intValue ? intValue : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Lee un valor bool del registro
        /// </summary>
        public bool GetBool(string valueName, bool defaultValue = false)
        {
            try
            {
                var key = _subKey ?? _rootKey.OpenSubKey(_basePath);
                if (key == null) return defaultValue;

                var value = key.GetValue(valueName, defaultValue ? 1 : 0);
                return value is int intValue && intValue != 0;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Escribe un valor en el registro
        /// </summary>
        public bool SetValue(string valueName, object value, RegistryValueKind? valueKind = null)
        {
            try
            {
                // Asegurarnos de tener una subclave escrita
                var key = _subKey ?? _rootKey.CreateSubKey(_basePath);
                if (key == null) return false;

                if (valueKind.HasValue)
                    key.SetValue(valueName, value, valueKind.Value);
                else
                    key.SetValue(valueName, value);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Escribe un valor string en el registro
        /// </summary>
        public bool SetString(string valueName, string value) 
            => SetValue(valueName, value, RegistryValueKind.String);

        /// <summary>
        /// Escribe un valor int en el registro
        /// </summary>
        public bool SetInt(string valueName, int value) 
            => SetValue(valueName, value, RegistryValueKind.DWord);

        /// <summary>
        /// Escribe un valor bool en el registro
        /// </summary>
        public bool SetBool(string valueName, bool value) 
            => SetValue(valueName, value ? 1 : 0, RegistryValueKind.DWord);

        /// <summary>
        /// Elimina un valor del registro
        /// </summary>
        public bool DeleteValue(string valueName, bool throwOnMissingValue = false)
        {
            try
            {
                var key = _subKey ?? _rootKey.OpenSubKey(_basePath, true);
                if (key == null) return false;

                key.DeleteValue(valueName, throwOnMissingValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si existe un valor
        /// </summary>
        public bool ValueExists(string valueName)
        {
            try
            {
                var key = _subKey ?? _rootKey.OpenSubKey(_basePath);
                return key?.GetValue(valueName) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si existe la subclave
        /// </summary>
        public bool KeyExists()
        {
            try
            {
                var key = _subKey ?? _rootKey.OpenSubKey(_basePath);
                return key != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene todos los nombres de valores en la subclave
        /// </summary>
        public string[] GetValueNames()
        {
            try
            {
                var key = _subKey ?? _rootKey.OpenSubKey(_basePath);
                return key?.GetValueNames() ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Obtiene todos los valores de la subclave como diccionario
        /// </summary>
        public Dictionary<string, object?> GetAllValues()
        {
            var result = new Dictionary<string, object?>();
            
            try
            {
                var key = _subKey ?? _rootKey.OpenSubKey(_basePath);
                if (key == null) return result;

                foreach (var valueName in key.GetValueNames())
                {
                    result[valueName] = key.GetValue(valueName);
                }
            }
            catch
            {
                // Retorna diccionario vacío o parcial en caso de error
            }

            return result;
        }

        /// <summary>
        /// Elimina la subclave completa
        /// </summary>
        public bool DeleteKey(bool recursive = false)
        {
            try
            {
                if (recursive)
                    _rootKey.DeleteSubKeyTree(_basePath, false);
                else
                    _rootKey.DeleteSubKey(_basePath, false);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene los nombres de las subclaves
        /// </summary>
        public string[] GetSubKeyNames()
        {
            try
            {
                var key = _subKey ?? _rootKey.OpenSubKey(_basePath);
                return key?.GetSubKeyNames() ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Crea un helper para una subclave relativa a la ruta base actual
        /// </summary>
        public RegistryHelper GetSubKey(string subKeyPath)
        {
            var newPath = string.IsNullOrEmpty(_basePath) 
                ? subKeyPath 
                : $"{_basePath}\\{subKeyPath}";
            
            return new RegistryHelper(_rootKey, newPath);
        }

        /// <summary>
        /// Libera recursos y cierra la subclave interna si fue abierta.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                try
                {
                    _subKey?.Close();
                    _subKey?.Dispose();
                }
                catch
                {
                    // Ignorar errores en dispose
                }
            }

            _disposed = true;
        }
    }
}

// ============= EJEMPLO DE USO =============
/*

using RegistryHelpers;

// Crear instancia para tu aplicación
var registry = RegistryHelper.CurrentUser(@"Software\MiAplicacion");

// Escribir valores
registry.SetString("Usuario", "Juan");
registry.SetInt("Contador", 42);
registry.SetBool("Activo", true);

// Leer valores
string usuario = registry.GetString("Usuario", "Desconocido");
int contador = registry.GetInt("Contador", 0);
bool activo = registry.GetBool("Activo", false);

// Verificar existencia
if (registry.ValueExists("Usuario"))
{
    Console.WriteLine("El usuario existe");
}

// Listar todos los valores
var valores = registry.GetAllValues();
foreach (var kvp in valores)
{
    Console.WriteLine($"{kvp.Key} = {kvp.Value}");
}

// Trabajar con subclaves
var config = registry.GetSubKey("Configuracion");
config.SetString("Tema", "Oscuro");

// Eliminar valor
registry.DeleteValue("Temporal");

// Para HKEY_LOCAL_MACHINE (requiere permisos de admin)
var systemRegistry = RegistryHelper.LocalMachine(@"Software\MiAplicacion");
systemRegistry.SetString("Version", "1.0.0");

*/