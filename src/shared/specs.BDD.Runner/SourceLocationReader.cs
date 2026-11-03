using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace specs.BDD.Runner
{
    /// <summary>
    /// Reads portable PDB files to find the source file path and line number for a method,
    /// so that test explorers can navigate to source.
    /// </summary>
    internal static class SourceLocationReader
    {
        /// <summary>
        /// Source file location for a method: file path and 1-based line number.
        /// </summary>
        public sealed class SourceLocation
        {
            /// <summary>Absolute path to the source file.</summary>
            public string FilePath { get; }

            /// <summary>1-based line number of the method's first statement.</summary>
            public int LineNumber { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SourceLocation"/> class.
            /// </summary>
            /// <param name="filePath">Absolute path to the source file.</param>
            /// <param name="lineNumber">1-based line number of the method's first statement.</param>
            public SourceLocation(string filePath, int lineNumber)
            {
                FilePath = filePath;
                LineNumber = lineNumber;
            }
        }

        /// <summary>
        /// Attempts to locate the source file and line number for <paramref name="method"/>
        /// by reading the portable PDB that accompanies <paramref name="assemblyPath"/>.
        /// </summary>
        /// <param name="assemblyPath">
        /// Absolute path to the assembly DLL. The PDB is expected alongside it with the same base name.
        /// </param>
        /// <param name="method">The method whose source location is needed.</param>
        /// <returns>
        /// A <see cref="SourceLocation"/> if the PDB was found and the method has sequence-point
        /// information; <c>null</c> otherwise. Never throws — any failure degrades gracefully.
        /// </returns>
        public static SourceLocation? TryGetLocation(string assemblyPath, MethodInfo method)
        {
            if (string.IsNullOrEmpty(assemblyPath) || method == null)
                return null;

            // A metadata token from another assembly would resolve to an unrelated method in this PDB.
            if (!string.Equals(
                    Path.GetFileName(method.Module.Assembly.Location),
                    Path.GetFileName(assemblyPath),
                    StringComparison.OrdinalIgnoreCase))
                return null;

            // Async/iterator bodies' sequence points live on the state machine's MoveNext.
            var stateMachineType = method.GetCustomAttribute<System.Runtime.CompilerServices.StateMachineAttribute>()?.StateMachineType;
            var moveNext = stateMachineType?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (moveNext != null)
                method = moveNext;

            var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
            if (!File.Exists(pdbPath))
                return null;

            try
            {
                using var pdbStream = File.OpenRead(pdbPath);
                using var provider = MetadataReaderProvider.FromPortablePdbStream(pdbStream);
                var reader = provider.GetMetadataReader();

                var handle = MetadataTokens.MethodDefinitionHandle(method.MetadataToken);
                var debugInfo = reader.GetMethodDebugInformation(handle);

                foreach (var sp in debugInfo.GetSequencePoints())
                {
                    // Skip hidden sequence points (compiler-generated, no source mapping)
                    if (sp.IsHidden) continue;
                    if (sp.Document.IsNil) continue;

                    var doc = reader.GetDocument(sp.Document);
                    var filePath = reader.GetString(doc.Name);
                    return new SourceLocation(filePath, sp.StartLine);
                }
            }
            catch
            {
                // Degrade gracefully if the PDB is embedded, corrupt, or in classic format.
                // "No source available" is better than a discovery crash.
            }

            return null;
        }

        /// <summary>
        /// Returns the user-authored method behind an It[...] action, unwrapping wrappers
        /// (async tests, ShouldThrow verifiers) that are bound to the delegate they wrap.
        /// </summary>
        public static MethodInfo GetTestMethod(Action action)
        {
            Delegate current = action;
            while (current.Target is Delegate inner)
                current = inner;
            return current.Method;
        }
    }
}
