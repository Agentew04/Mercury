---
title: Chamadas de Sistema
---

# Chamadas de Sistema

Este documento irá listar todas as chamadas de sistema disponíveis para uso e sua
utilização para o sistema operacional **Mars** com arquitetura **MIPS**.

| Nome | Número | Descrição |
|------|--------|-----------|
| Imprimir Inteiro | 1 | Imprime o número inteiro presente em `$a0` para saída padrão. |
| Imprimir Ponto Flutuante 32 bits | 2 | Imprime o número de ponto flutuante presente em `$f12` para saída padrão. |
| Imprimir Ponto Flutuante 64 bits | 3 | Imprime o número de ponto flutuante presente em `$f12` para saída padrão. |
| Imprimir Cadeia de Caracteres | 4 | Imprime uma cadeia terminada em `\0` cujo endereço base está presente em `$a0`. |
| Ler inteiro | 5 | Lê um número inteiro da entrada padrão e o armazena em `$v0`. |
| Ler Ponto Flutuante 32 bits | 6 | Lê um número de ponto flutuante da entrada padrão e o armazena em `$f0`. |
| Ler Ponto Flutuante 64 bits | 7 | Lê um número de ponto flutuante da entrada padrão e o armazena em `$f0`. |
| Ler Cadeia de Caracteres | 8 | Lê uma cadeia de caracteres da entrada padrão e a armazena no endereço base presente em `$a0`. `$a1` contém o tamanho do buffer (lê `$a1` - 1 caracteres). |
| SBRK | 9 | Aloca memória dinâmica. O valor presente em `$a0` é o número de bytes a serem alocados. O endereço base do bloco alocado é retornado em `$v0`. |
| Saída | 10 | Termina a execução do programa. |
| Imprimir Caractere | 11 | Imprime o caractere presente em `$a0` para a saída padrão. |
| Ler Caractere | 12 | Lê um caractere da entrada padrão e o armazena em `$v0`. |
| Abrir arquivo | 13 | Abre um arquivo cujo nome está presente no endereço base em `$a0`. O modo de abertura é especificado em `$a1` (0 para leitura, 1 para escrita, 2 para leitura e escrita). O descritor do arquivo aberto é retornado em `$v0`. `$v0` é negativo caso um erro ocorreu. |
| Ler do arquivo | 14 | Lê uma quantia de caracteres de um arquivo aberto. `$a0` contém o descritor do arquivo. `$a1` contém o endereço base do buffer onde os dados lidos serão armazenados. `$a2` contém a quantidade de bytes a serem lidos. O número de bytes lidos é retornado em `$v0`, sendo negativo caso um erro ocorreu ou leu **EOF** |
| Escrever para arquivo | 15 | Escreve uma quantia de caracteres em um arquivo aberto. `$a0` contém o descritor do arquivo. `$a1` contém o endereço base do buffer que contém os dados a serem escritos. `$a2` contém a quantidade de bytes a serem escritos. O número de bytes escritos é retornado em `$v0`, sendo negativo caso um erro ocorreu. |
| Fechar arquivo | 16 | Fecha um arquivo aberto. `$a0` contém o descritor do arquivo a ser fechado. |
| Saída com Valor | 17 | Termina a execução do programa com `$a0` como código de saída. |
| -- | -- | Chamadas menores que 17 são compatíveis com simulador SPIM também. |
| Tempo do Sistema | 30 | Retorna o tempo atual do sistema. `$a0` contém os 32 bits mais baixos do tempo atual, enquanto `$a1` contém os 32 bits mais altos. |
| Tocar som assíncrono | 31 | Toma um som no sistema e retorna imediatamente. `$a0` contém o tom (0-127), `$a1` contém a duração do som em milissegundos. `$a2` contém o instrumento (0-127) e `$a3` contém o volume (0-127) |
| Esperar | 32 | Espera o tempo presente em `$a0` em milissegundos. |
| Tocar som síncrono | 33 | Toca um som e retorna quando o som terminar. Mesmos parâmetros da chamada `31`. |
| Imprimir Inteiro Hexadecimal | 34 | Imprime o número inteiro presente em `$a0` como um valor hexadecimal. |
| Imprimir Inteiro Binário | 35 | Imprime o número inteiro presente em `$a0` como um valor binário. Sempre tem 32 caracteres de largura com 0s à esquerda. |
| Imprimir Inteiro sem Sinal | 36 | Imprime o número inteiro sem sinal presente em `$a0`. |
| Definir semente aleatória | 40 | Define a semente do gerador de números aleatórios. `$a0` o id do gerador e `$a1` contém a semente. |
| Gerar número aleatório | 41 | Gera um número aleatório. `$a0` contém o id do gerador. O número gerado é retornado em `$a0`. |
| Gerar número aleatório no intervalo | 42 | Gera um número aleatório no intervalo [0,N]. `$a0` contém o id do gerador e `$a1` o limite superior. Valor retornado pelo `$a0`. |
| Gerar ponto flutuante 32 bits aleatório | 43 | Gera um número de ponto flutuante 32 bits aleatório no intervalo [0,1]. `$a0` contém o id do gerador. O número gerado é retornado em `$f0`. |
| Gerar ponto flutuante 64 bits aleatório | 44 | Gera um número de ponto flutuante 64 bits aleatório no intervalo [0,1]. `$a0` contém o id do gerador. O número gerado é retornado em `$f0`. |
| Imprimir Booleano | 45 | Imprime um valor booleano na saída padrão. `$a0` contém o valor booleano. |
| Ler Booleano | 46 | Lê um valor booleano da entrada padrão e o armazena em `$v0`. |