<!--
SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>

SPDX-License-Identifier: MIT
-->

Alad.CodeAnalyzer
=================

Code-analyzer di Alad, controlla che il codice rispetti le convenzioni di Alad
e cerca di prevenire i più comuni errori di programmazione.


## Utilizzo

È possibile installare il code-analyzer di Alad come estensione di Visual Studio
per attivarlo su tutti i progetti sviluppati, oppure come pacchetto NuGet per
attivarlo sui progetti individuali.

### Estensione di Visual Studio

Aprire _Strumenti » Opzioni... » Ambiente » Estensioni_, ed aggiungere una nuova
estensione nella sezione _Raccolte di estensioni aggiuntive_.

```plaintext
Name: Alad.cloud
URL: https://alad00.github.io/Alad.CodeAnalyzer/feed.xml
```

Confermare con _OK_, ed aprire _Estensioni » Gestisci le estensioni_.

Da questa finestra espandere _Online » Alad.cloud_ ed installare
_Alad.CodeAnalyzer_.

### Pacchetto NuGet

Chiudere Visual Studio ed aggiungere un file _NuGet.Config_ nella root del
progetto. Il contenuto del file è il seguente.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="alad" value="https://nuget.pkg.github.com/alad00/index.json" protocolVersion="3" />
  </packageSources>
  <packageSource key="alad">
    <package pattern="Alad.*" />
  </packageSource>
</configuration>
```

Aprire Visual Studio, fare click destro sulla soluzione, e selezionare
_Gestisci pacchetti NuGet per la soluzione..._.

Dalla tab _Sfoglia_ cercare _Alad.CodeAnalyzer_ ed installarlo nel progetto.


## Licenza

Vedere [LICENSE](LICENSE).
