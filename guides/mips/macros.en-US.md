---
title: Macros
---

# Macros

Macros are a mixture of functions and #defines in C/C++ and consist of small, parameterizable, and reusable code blocks. They are primarily used to simplify written code and improve readability. Macros are resolved at compile time and are essentially copied and pasted into the user's code, replacing parameters as needed.

> **Note:** Macros are not functions. They have no scope, do not check types, and do not return values. They are simply text substitutions.

## How to use in MIPS Assembly
Macros in MIPS Assembly are defined using the `.macro` directive and ended with the `.endmacro` directive. The basic syntax for defining a macro is as follows:

```assembly
.macro macro_name param1, param2, ...
    # Macro body
    instructions that use param1, param2, etc.
.endmacro
```

Remember that to reference a parameter in the macro body, the name must be prefixed with a backslash (`\`).

## Examples

```asm
.macro sum a, b, result
    add \result, \a, \b
.endmacro
# ...
sum $t0, $t1, $t2  # This expands to: add $t2, $t0, $t1
```

```asm
.macro printLiteral literal
    .data
        msg: .asciiz \literal
    .text
    li $v0, 4          # Service code to print string
    la $a0, msg        # Load string address
    syscall            # Call operating system
.endmacro
# ...
printLiteral "Hello, World!"
```

```asm
.macro mod rs, n, rd
    li \rd, n
    div \rs, \rd
    mfhi \rd
.endmacro
# ...
mod $t0, 5, $t1  # t1 <= t0 % 5
```
