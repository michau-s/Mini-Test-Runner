# MiniTestRunner

## Description
MiniTestRunner was developed as part of the Programming 3 - Advanced course during the winter semester of 2024/2025 academic year.

It aims to be a lightweight unit test framework working in the console, supporting custom attributes and assertions to manage the test's lifecycle.

## Running the program
Simply compile the project and run `./MiniTestRunner <path-to-test-assembly.dll>` with an absolute path as the program's argument.

For example, you can run it on the included `AuthenticationService.Tests.dll"` library:

```
./MiniTestRunner ~\MiniTestFramework\MiniTest\AuthenticationService.Tests\bin\Debug\net8.0\AuthenticationService.Tests.dll
```

Which should look like this:

<p align="center">
  <img src=".\Docs\Images\ConsoleOutput.png" 
       alt="Console Output" 
       style="width: 80%;"/>
</p>

For the guide on how to mark methods and classes as test containers, see [MiniTest](#minitest).

## Implementation
Four primary classes have been implemented:
- **TestLoader** - Provides an interface for loading the tests, scans the `.dll` for classes and methods marked with specified attributes and stores them in **TestData**.
- **TestRunner** - Provides an interface for running test a stored in **TestData**.
- **OutputFormatter** - An output formatter, used to quickly output results, warnings and summaries to the console. 
- **TestData** - A class used to store and encapsulate a single test.

You are encouraged to take a look at the XML documentation for supplementary information about the implementation. Additionally, the original task description has been included below for convenience.

## MiniTest

### Test Attributes

The library should provide the following attributes used to mark classes and methods as test containers and manage the test lifecycle.

1. **TestClassAttribute** marks a class as a container for test methods.
2. **TestMethodAttribute** marks a method as a unit test to be executed.
3. **BeforeEachAttribute** defines a method to be executed before each test method.
4. **AfterEachAttribute** defines a method to be executed after each test method.
5. **PriorityAttribute** sets a priority (integer) for test prioritization, with lower numerical values indicating higher priority.
6. **DataRowAttribute** enables parameterized testing by supplying data to test methods.
   - it should accept an array of objects (`object?[]`) representing test data.
   - optionally takes a string parameter that documents the test data set.
7. **DescriptionAttribute** allows the inclusion of additional description to a test or a test class.
   
### Assertions

The library also provides methods to verify test success or failure conditions.
It is handled by a static class `Assert`, which includes methods that will handle assertions.

1. `ThrowsException<TException>(Action action, string message = "")`: Confirms that a specific exception type is thrown during a given operation.
2. `AreEqual<T>(T? expected, T? actual, string message = "")`: Verifies equality between expected and actual values.
3. `AreNotEqual<T>(T? notExpected, T? actual, string message = "")`: Ensures that the expected and actual values are distinct.
4. `IsTrue(bool condition, string message = "")`: Confirms that a boolean condition is true.
5. `IsFalse(bool condition, string message = "")`: Confirms that a boolean condition is false.
6. `Fail(string message = "")`: Explicitly fails a test with a custom error message.