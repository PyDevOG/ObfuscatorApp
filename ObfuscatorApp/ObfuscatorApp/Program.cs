using System;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace ObfuscatorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ObfuscatorApp.exe <input_file> <output_file>");
                return;
            }

            string inputFile = args[0];
            string outputFile = args[1];

            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Error: Input file '{inputFile}' does not exist.");
                return;
            }

            try
            {
                Console.WriteLine($"Starting obfuscation for: {inputFile}");
                Obfuscator.PerformObfuscation(inputFile, outputFile);
                Console.WriteLine($"Obfuscation completed successfully. Output: {outputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during obfuscation: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    public static class Obfuscator
    {
        private static readonly Random random = new Random();

        public static void PerformObfuscation(string inputFile, string outputFile)
        {
            string tempFile = Path.Combine(Path.GetDirectoryName(inputFile), $"tmp_{Path.GetFileName(inputFile)}");

            try
            {
                File.Copy(inputFile, tempFile, overwrite: true);

                using (ModuleDef module = ModuleDefMD.Load(tempFile))
                {
                    Console.WriteLine("Applying RenameProtector...");
                    RenameProtector.Execute(module);

                    Console.WriteLine("Obfuscating Assembly Name...");
                    ObfuscateAssembly(module);

                    Console.WriteLine("Obfuscating References...");
                    ReferenceProtector.Execute(module);

                    Console.WriteLine("Obfuscating Namespaces...");
                    ObfuscateNamespaces(module);

                    Console.WriteLine("Adding JunkMethods...");
                    JunkMethods.Execute(module, 5, 5, 3);

                    Console.WriteLine("Obfuscating Metadata...");
                    MetadataObfuscator.Execute(module);

                    Console.WriteLine("Simplifying and Optimizing Methods...");
                    SimplifyAndOptimizeBranches(module);

                    Console.WriteLine("Writing the obfuscated module...");
                    var writerOptions = new ModuleWriterOptions(module)
                    {
                        MetadataOptions = { Flags = MetadataFlags.KeepOldMaxStack },
                        Logger = DummyLogger.NoThrowInstance
                    };

                    foreach (var type in module.Types)
                    {
                        foreach (var method in type.Methods)
                        {
                            if (method.HasBody)
                            {
                                method.Body.KeepOldMaxStack = true;
                            }
                        }
                    }

                    module.Write(outputFile, writerOptions);
                }

                Console.WriteLine("Obfuscation process complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Obfuscation failed: {ex.Message}\nFailed method: {ex.TargetSite}");
                throw;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private static void SimplifyAndOptimizeBranches(ModuleDef module)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (method.HasBody)
                    {
                        method.Body.SimplifyBranches();
                        method.Body.OptimizeBranches();
                    }
                }
            }
        }

        private static void ObfuscateAssembly(ModuleDef module)
        {
            module.Name = RandomString(15); 
            if (module.Assembly != null)
            {
                module.Assembly.Name = RandomString(15); 
                module.Assembly.PublicKey = null; 
                module.Assembly.Version = new Version(
                    random.Next(0, 10),
                    random.Next(0, 10),
                    random.Next(0, 10),
                    random.Next(0, 10));
            }
        }

        private static void ObfuscateNamespaces(ModuleDef module)
        {
            foreach (var type in module.Types)
            {
                if (!type.IsGlobalModuleType)
                {
                    type.Namespace = RandomString(10); 
                }
            }
        }

        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "0123456789" + "∪ ƒ ∫ ∬ ∭ ∮ ∯ ∰ ∱ ∲ ∳" + "⋦ ⋧ ⋨ ⋩ ⋪ ⋫ ⋬ ⋭ ⋮ ⋯ ⋰ ⋱" + "!@#$%^&*()_-+={[}]|:;<,>.?" + "Ցց Ււ Փփ Քք Օօ Ֆֆ Φ φ Χ χ Ψ ψ Ω ω ρ Σ σ ς Τ τ Υ Ϋ υ ϋ (ם) נ (ן) ס ע פ (ף) צ (ץ) ק ר ש ת";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static class ReferenceProtector
        {
            public static void Execute(ModuleDef module)
            {
                foreach (var assemblyRef in module.GetAssemblyRefs())
                {
                    string originalName = assemblyRef.Name;

               
                    if (originalName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                        originalName.Equals("System", StringComparison.OrdinalIgnoreCase) ||
                        originalName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
                        assemblyRef.PublicKeyOrToken != null)
                    {
                        Console.WriteLine($"Skipping critical or strongly named reference: {originalName}");
                        continue;
                    }

                  
                    string newName = RandomString(10);
                    Console.WriteLine($"Obfuscating reference: {originalName} -> {newName}");
                    assemblyRef.Name = newName;

            
                    assemblyRef.Version = null;

                   
                    assemblyRef.PublicKeyOrToken = null;
                }
            }

            private static string RandomString(int length)
            {
                const string chars = "abcdefghijklmnopqrstuvwxyz" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "0123456789" + "∪ ƒ ∫ ∬ ∭ ∮ ∯ ∰ ∱ ∲ ∳" + "⋦ ⋧ ⋨ ⋩ ⋪ ⋫ ⋬ ⋭ ⋮ ⋯ ⋰ ⋱" + "!@#$%^&*()_-+={[}]|:;<,>.?" + "Ցց Ււ Փփ Քք Օօ Ֆֆ Φ φ Χ χ Ψ ψ Ω ω ρ Σ σ ς Τ τ Υ Ϋ υ ϋ (ם) נ (ן) ס ע פ (ף) צ (ץ) ק ר ש ת";
                return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }


        public static class MetadataObfuscator
        {
            public static void Execute(ModuleDef module)
            {
                module.Name = RandomString(15);
                if (module.Assembly != null)
                {
                    module.Assembly.Name = RandomString(15);
                    module.Assembly.PublicKey = null;
                    module.Assembly.Version = new Version(
                        random.Next(0, 10),
                        random.Next(0, 10),
                        random.Next(0, 10),
                        random.Next(0, 10));
                }

                foreach (var attr in module.Assembly?.CustomAttributes ?? Enumerable.Empty<CustomAttribute>())
                {
                    attr.Constructor.Name = RandomString(10);
                    attr.ConstructorArguments.Clear();
                }
            }

            private static string RandomString(int length)
            {
                const string chars = "abcdefghijklmnopqrstuvwxyz" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "0123456789" + "∪ ƒ ∫ ∬ ∭ ∮ ∯ ∰ ∱ ∲ ∳" + "⋦ ⋧ ⋨ ⋩ ⋪ ⋫ ⋬ ⋭ ⋮ ⋯ ⋰ ⋱" + "!@#$%^&*()_-+={[}]|:;<,>.?" + "Ցց Ււ Փփ Քք Օօ Ֆֆ Φ φ Χ χ Ψ ψ Ω ω ρ Σ σ ς Τ τ Υ Ϋ υ ϋ (ם) נ (ן) ס ע פ (ף) צ (ץ) ק ר ש ת";
                return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }

        public static class RenameProtector
        {
            public static void Execute(ModuleDef module)
            {
                foreach (var type in module.Types)
                {
                    if (!type.IsGlobalModuleType)
                    {
                        type.Name = RandomString(20);

                        foreach (var method in type.Methods)
                        {
                            if (!method.IsConstructor)
                                method.Name = RandomString(20);
                        }

                        foreach (var field in type.Fields)
                        {
                            field.Name = RandomString(20);
                        }
                    }
                }
            }

            private static string RandomString(int length)
            {
                const string chars = "abcdefghijklmnopqrstuvwxyz" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "0123456789" + "∪ ƒ ∫ ∬ ∭ ∮ ∯ ∰ ∱ ∲ ∳" + "⋦ ⋧ ⋨ ⋩ ⋪ ⋫ ⋬ ⋭ ⋮ ⋯ ⋰ ⋱" + "!@#$%^&*()_-+={[}]|:;<,>.?" + "Ցց Ււ Փփ Քք Օօ Ֆֆ Φ φ Χ χ Ψ ψ Ω ω ρ Σ σ ς Τ τ Υ Ϋ υ ϋ (ם) נ (ן) ס ע פ (ף) צ (ץ) ק ר ש ת";
                return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }

        public static class JunkMethods
        {
            public static void Execute(ModuleDef module, int junkClasses, int junkMethodsPerClass, int junkInstructionsPerMethod)
            {
                for (int i = 0; i < junkClasses; i++)
                {
                    var junkType = new TypeDefUser(RandomString(10), module.CorLibTypes.Object.TypeDefOrRef);
                    module.Types.Add(junkType);

                    for (int j = 0; j < junkMethodsPerClass; j++)
                    {
                        var junkMethod = new MethodDefUser(RandomString(10), MethodSig.CreateStatic(module.CorLibTypes.Void), MethodAttributes.Public | MethodAttributes.Static)
                        {
                            Body = new CilBody { KeepOldMaxStack = true }
                        };

                        for (int k = 0; k < junkInstructionsPerMethod; k++)
                        {
                            junkMethod.Body.Instructions.Add(GenerateRandomInstruction());
                        }

                        junkMethod.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
                        junkType.Methods.Add(junkMethod);
                    }
                }
            }

            private static Instruction GenerateRandomInstruction()
            {
                var opCodes = new[]
                {
                    OpCodes.Nop,
                    OpCodes.Ldc_I4,
                    OpCodes.Add,
                    OpCodes.Sub,
                    OpCodes.Mul,
                    OpCodes.Div,
                    OpCodes.Pop
                };

                var selectedOpCode = opCodes[random.Next(opCodes.Length)];
                if (selectedOpCode == OpCodes.Ldc_I4)
                {
                    return Instruction.Create(selectedOpCode, random.Next(0, 100));
                }

                return Instruction.Create(selectedOpCode);
            }

            private static string RandomString(int length)
            {
                const string chars = "abcdefghijklmnopqrstuvwxyz" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "0123456789" + "∪ ƒ ∫ ∬ ∭ ∮ ∯ ∰ ∱ ∲ ∳" + "⋦ ⋧ ⋨ ⋩ ⋪ ⋫ ⋬ ⋭ ⋮ ⋯ ⋰ ⋱" + "!@#$%^&*()_-+={[}]|:;<,>.?" + "Ցց Ււ Փփ Քք Օօ Ֆֆ Φ φ Χ χ Ψ ψ Ω ω ρ Σ σ ς Τ τ Υ Ϋ υ ϋ (ם) נ (ן) ס ע פ (ף) צ (ץ) ק ר ש ת";
                return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }
    }
}
