<!--
SPDX-FileCopyrightText: 2025 ALAD SRL <info@alad.cloud>

SPDX-License-Identifier: MIT
-->

Alad.CodeAnalyzer
=================

## 0.10.0 (DRAFT)
* [ALAD0110](docs/codes/ALAD0110.md) (nuovo): API sperimentali
* [ALAD1008](docs/codes/ALAD1008.md) (nuovo): caratteri speciali nei nomi
* [ALAD0220](docs/codes/ALAD0220.md): fix campi statici pubblici readonly per le
  istanze di oggetti predefinite

## 0.9.3
* [ALAD0001](docs/codes/ALAD0001.md): fix eccezione cast non valido quando sono
  presenti attributi
* [ALAD0400](docs/codes/ALAD0400.md): fix messaggio
* [ALAD1005](docs/codes/ALAD1005.md): fix trigger anomalo su `record struct`

## 0.9.2
* [ALAD0220](docs/codes/ALAD0220.md): gestione campi `readonly`

## 0.9.1
* [ALAD0400](docs/codes/ALAD0400.md): controlli su `DbContext`, fix link alla
  documentazione
* [ALAD0401](docs/codes/ALAD0401.md): fix doppio using, fix link alla
  documentazione

## 0.9.0
* [ALAD0400](docs/codes/ALAD0400.md) (nuovo): chiamata senza await a metodo che
  non supporta operazioni in parallelo
* [ALAD0401](docs/codes/ALAD0401.md) (nuovo): metodo che non supporta operazioni
  in parallelo

## 0.8.5
* [ALAD0002](docs/codes/ALAD0002.md): fix crash con espressioni `new()`
  (creazione di oggetti con tipo implicito)
* [ALAD1005](docs/codes/ALAD1005.md): soppressione warning quando si fa
  `override` di un metodo che non rispetta le convenzioni
* [ALAD1005](docs/codes/ALAD1005.md): soppressione warning quando si implementa
  esplicitamente un'interfaccia che non rispetta le convenzioni

## 0.8.4
* [ALAD0302](docs/codes/ALAD0302.md): fix trigger anomalo su parametri
  all'inizio
* Fix lingua estensione per Visual Studio, da `en-US` ad `it-IT`

## 0.8.3
* [ALAD1002](docs/codes/ALAD1002.md): fix trigger anomalo su indexer

## 0.8.2
* [ALAD1004](docs/codes/ALAD1004.md): fix trigger anomalo su `const`

## 0.8.1
* [ALAD1002](docs/codes/ALAD1002.md): fix trigger anomalo su metodi `operator`

## 0.8.0
* [ALAD1006](docs/codes/ALAD1006.md) (nuovo): nome namespace non rispetta le
  convenzioni

## 0.7.0
* [ALAD1002](docs/codes/ALAD1002.md) (nuovo): nome field pubblico non rispetta
  le convenzioni
* [ALAD1004](docs/codes/ALAD1004.md) (nuovo): nome field statico privato non
  rispetta le convenzioni
* [ALAD1005](docs/codes/ALAD1005.md) (nuovo): nome parametro non rispetta le
  convenzioni
* [ALAD1007](docs/codes/ALAD1007.md) (nuovo): nome costante non rispetta le
  convenzioni
* [ALAD1003](docs/codes/ALAD1003.md): fix trigger anomalo su campi `static`

## 0.6.0
* [ALAD1003](docs/codes/ALAD1003.md) (nuovo): nome field privato non rispetta le
  convenzioni

## 0.5.0
* [ALAD0300](docs/codes/ALAD0300.md) (nuovo): log di un singolo evento suddiviso
  su più righe
* [ALAD0301](docs/codes/ALAD0301.md) (nuovo): log in un blocco catch senza
  eccezione
* [ALAD0302](docs/codes/ALAD0302.md) (nuovo): log ornamentale
* [ALAD0220](docs/codes/ALAD0220.md): fix trigger anomalo su campi `const` ed
  `enum`

## 0.4.0
Prima versione pubblica.
