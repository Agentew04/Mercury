---
title: Laços de Repetição
---

# Laços de Repetição


Laços de repetição são essenciais para controle de fluxo em um programa. Em assembly, eles são criados a partir da organização de instruções de desvio condicional e incondicional.

## Laços `while`

Os laços `while`, ou "enquanto", são os mais simples que existem, e todos os outros tipos podem ser simplificados para ele. Estes laços são representados em código C utilizando a _keyword_ `while`:
```c
while(condição){
    corpo
}
```

Para transformar o laço em um conjunto linear de instruções, basta adicionar um desvio condicional ao início do corpo e um desvio incondicional ao final:

```
início:
se !condição vai para fim
corpo
vai para início
fim:
```

Traduzindo isso para instruções MIPS, temos:

```mips
# rótulo de início
while_start:
    # calcula condição se for uma expressão complexa
    # assumindo que $t0 contém o valor booleano
    beq $t0, $zero, while_end

    # corpo do laço

    # volta ao início
    j while_start
while_end:
```




## Laços `for`

Os laços `for` tem quatro partes fundamentais:
```
for(inicialização;condição;pós-execução){
    corpo
}
```
Que são linearizadas para:
```
inicialização
while(condição){
    corpo
    pós-execução
}
```

## Laços `do-while`