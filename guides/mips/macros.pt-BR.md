---
title: Macros
---

# Macros

Macros são uma mistura de funções e #defines em C/C++ e consistem em pequenos blocos de código parametrizáveis e reutilizáveis. São usados principalmente para simplificar o código escrito e melhorar a legibilidade. Os macros são resolvidos em tempo de compilação e são basicamente copiados e colados no código do usuário, substituindo parâmetro conforme necessário.

> **Nota:** Macros não são funções. Eles não têm escopo, não verificam tipos e não retornam valores. Eles são simplesmente substituições de texto.

## Como utilizar no Assembly MIPS
Macros no Assembly MIPS são definidos usando a diretiva `.macro` e terminados com a diretiva `.endmacro`. A sintaxe básica para definir um macro é a seguinte:

```assembly
.macro nome_do_macro param1, param2, ...
    # Corpo do macro
    instruções que usam param1, param2, ...
.endmacro
```

Lembrando que para referenciar um parâmetro no corpo do macro, o nome deve ser prefixado com uma barra invertida (`\`).

## Exemplos

```asm
.macro soma a, b, resultado
    add \resultado, \a, \b
.endmacro
# ...
soma $t0, $t1, $t2  # Isso expande para: add $t2, $t0, $t1
```

```asm
.macro imprimeLiteral literal
    .data
        msg: .asciiz \literal
    .text
    li $v0, 4          # Código do serviço para imprimir string
    la $a0, msg        # Carrega o endereço da string
    syscall            # Chama o serviço do sistema
.endmacro
# ...
imprimeLiteral "Olá, Mundo!"
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
