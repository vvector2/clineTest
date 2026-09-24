# C# Code Style & Conventions

When writing or modifying C# code, strictly adhere to the following coding standards and guidelines.

---

## 1. Naming Conventions

- **PascalCase**: Classes, Structs, Enums, Interfaces, Methods, Properties, Events, and Public Fields.
- **Interface Prefix**: Always prefix interfaces with `I` (e.g., `IUserRepository`).
- **camelCase**: Local variables and method parameters.
- **Private Instance Fields**: Use `_` prefix with camelCase (e.g., `_logger`, `_userRepository`).
- **Private Static Fields**: Use `s_` prefix with camelCase (e.g., `s_instance`).
- **Constants**: Use PascalCase (e.g., `MaxRetries = 3`).

---

## 2. Code Structure & Layout

- **Brace Style (Allman)**: Open and close braces on their own line, aligned with the block.
- **Indentation**: 4 spaces (no tab characters).
- **Class Member Ordering**:
  1. Constants & Static fields
  2. Private `readonly` fields
  3. Constructors
  4. Public properties and methods
  5. Private/protected helper methods

---

## 3. Language Features & Best Practices

- **Built-in Type Aliases**: Use native C# keywords (`string`, `int`, `bool`, `object`) instead of .NET type names (`String`, `Int32`, `Boolean`, `Object`).
- **Implicit Typing (`var`)**: Use `var` only when the explicit type is obvious from the right-hand assignment (e.g., `var list = new List<string>();`). Use explicit types when the returning type is ambiguous.
- **String Formatting**: Prefer string interpolation (`$"Hello, {name}"`) over `string.Format()` or concatenation.
- **Null Safety**:
  - Use C# nullable reference types (`string?`).
  - Use pattern matching (`if (obj is null)`) or guard clauses (`ArgumentNullException.ThrowIfNull(param)`).
- **Expression-Bodied Members**: Use `=>` for single-line read-only properties, simple methods, or constructor assignments where clarity is improved.

---

## 4. Async & Modern C# Guidelines

- **Async/Await**: Append `Async` to asynchronous method names (e.g., `GetUserByIdAsync`).
- **Cancellation Tokens**: Always accept a `CancellationToken cancellationToken = default` parameter in async APIs and pass it down.
- **File-Scoped Namespaces**: Prefer file-scoped namespaces for lower indentation overhead:

  ```csharp
  namespace MyApp.Services;

  public class UserService
  {
      // ...
  }
  ```

- **Comments**: Do not add comments
