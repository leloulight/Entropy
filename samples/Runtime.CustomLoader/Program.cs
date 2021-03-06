using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Runtime.CustomLoader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Use the default load context
            var loadContext = PlatformServices.Default.AssemblyLoadContextAccessor.Default;

            // Add the loader to the container so that any call to Assembly.Load will
            // call the load context back (if it's not already loaded)
            using (PlatformServices.Default.AssemblyLoaderContainer.AddLoader(new DirectoryLoader(@"", loadContext)))
            {
                // You should be able to use Assembly.Load()
                var assembly1 = Assembly.Load(new AssemblyName("SomethingElse"));

                // Or call load on the context directly
                var assembly2 = loadContext.Load("SomethingElse");

                foreach (var definedType in assembly1.DefinedTypes)
                {
                    Console.WriteLine("Found type {0}", definedType.FullName);
                }

                Console.ReadLine();
            }
        }
    }

    public class DirectoryLoader : IAssemblyLoader
    {
        private readonly IAssemblyLoadContext _context;
        private readonly string _path;

        public DirectoryLoader(string path, IAssemblyLoadContext context)
        {
            _path = path;
            _context = context;
        }

        public Assembly Load(AssemblyName assemblyName)
        {
            return _context.LoadFile(Path.Combine(_path, assemblyName.Name + ".dll"));
        }

        public IntPtr LoadUnmanagedLibrary(string name)
        {
            throw new NotImplementedException();
        }
    }
}
