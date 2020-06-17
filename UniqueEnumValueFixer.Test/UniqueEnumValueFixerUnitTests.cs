using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using UniqueEnumValueFixer;

namespace UniqueEnumValueFixer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void Verify_No_Diagnostic_For_Empty_String()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Verify_Diagnostic_NotCreated_For_Enum_With_No_Duplicate_Values()
        {
            var test = @"
   using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
                public enum ImportFaultType{
    [EnumMember]
    Undefined = 0,
    [EnumMember]
    Unspecified = 1,
    [EnumMember]
    DuplicateForm = 2,
    [EnumMember]
    MappingError = 3,
    [EnumMember]
    PatientSearchError = 4,
    [EnumMember]
    MissingMainFormInImportSet = 5,
    [EnumMember]
    MissingMainFormInRegistry = 6,
    [EnumMember]
    PatientNotFound = 14,
}
            ";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Verify_CodeFix_Created_For_Enum_With_Duplicate_Values()
        {
            var test = @"using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
   
    public enum ImportFaultType{
    [EnumMember]
    Undefined = 0,
    [EnumMember]
    Unspecified = 1,
    [EnumMember]
    DuplicateForm = 2,
    [EnumMember]
    MappingError = 3,
    [EnumMember]
    PatientSearchError = 4,
    [EnumMember]
    MissingMainFormInImportSet = 5,
    [EnumMember]
    MissingMainFormInRegistry = 6,
    [EnumMember]
    PatientNotFound = 4,}
            ";
            var adjustedTestCodeAfterFix = @"using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
   
//TODO: Remove use of duplicate enum members pointing to same value?
public enum ImportFaultType{
    [EnumMember]
    Undefined = 0,
    [EnumMember]
    Unspecified = 1,
    [EnumMember]
    DuplicateForm = 2,
    [EnumMember]
    MappingError = 3,
    [EnumMember]
    PatientSearchError = 4,
    [EnumMember]
    MissingMainFormInImportSet = 5,
    [EnumMember]
    MissingMainFormInRegistry = 6,
    [EnumMember]
    PatientNotFound = 4,}
            ";
            VerifyCSharpFix(test, adjustedTestCodeAfterFix);
        }

        [TestMethod]
        public void Verify_Diagnostic_Created_For_Enum_With_Duplicate_Values()
        {
            var test = @"
   using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
                public enum ImportFaultType{
    [EnumMember]
    Undefined = 0,
    [EnumMember]
    Unspecified = 1,
    [EnumMember]
    DuplicateForm = 2,
    [EnumMember]
    MappingError = 3,
    [EnumMember]
    PatientSearchError = 4,
    [EnumMember]
    MissingMainFormInImportSet = 5,
    [EnumMember]
    MissingMainFormInRegistry = 6,
    [EnumMember]
    PatientNotFound = 4,
}
            ";

            var expected = new DiagnosticResult
            {
                Id = "UniqueEnumValueFixer",
                Message = String.Format("Enum with type '{0}' contains duplicate values.", "ImportFaultType"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 8, 29)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        //Diagnostic and CodeFix both triggered and checked for

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UniqueEnumValueFixerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UniqueEnumValueFixerAnalyzer();
        }
    }
}
