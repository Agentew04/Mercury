# Relatório de mudanças – Mercury

## Instruções novas

Implementei JAL, LH, SH, LUI, MFHI e MTHI. Cada uma virou uma classe seguindo o padrão das que já existiam (os atributos `[Instruction]`, `[FormatExact]` e `[Field]`) e ganhou seu `case` de execução na CPU: JAL no `Monocycle.TypeJ` (igual ao J, só que salvando o retorno em `$ra`), LH/SH/LUI no `Monocycle.TypeI` (LH e SH conferem o alinhamento do endereço, como o LHU já fazia) e MFHI/MTHI no `Monocycle.TypeR`.

## FPU separada da CPU

A parte de ponto flutuante morava dentro do `Monocycle`, no `Monocycle.TypeF.cs`. Tirei ela de lá e transformei num módulo próprio(como era o esperado), o `Runtime/Simple/Fpu.cs`, que continua usando os mesmos registradores e flags da CPU (compartilhados por referência). A lógica em si é a mesma de antes: add, sub, mul, div, sqrt, abs, neg, mov, cvt, as comparações.


## Testes

O engine compila limpo. Dos 378 testes, 374 passam; os 4 que ainda falham são os de cache (`TestSmallSplit*` e `TestBigSplit*`), que não têm nada a ver com o que mexi. O `ArithmeticBehaviourTest` e o `SystemBehaviourTest`, que já existiam mas dependiam do `WithAnonymousOs` para compilar, voltaram a rodar e agora passam.
