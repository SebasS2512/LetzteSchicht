===============================================================================
LETZTE SCHICHT
Ein First-Person-Horrorspiel in Unity
===============================================================================

Der Spieler uebernimmt die Nachtschicht als Hausmeister an einer Hochschule
und arbeitet vier Naechte lang seine Routine ab - waehrend das Gebaeude sich
von Nacht zu Nacht veraendert.


-------------------------------------------------------------------------------
1. PROJEKTTEILNEHMER
-------------------------------------------------------------------------------

  Name                        
  --------------------------  
  Gabriel Braun           
  Sebastian Steiner           

  Kurs:       Spieleprogrammierung und 3D-Animation
  Hochschule: Hochschule Osnabrueck
  Semester:   SoSe 2026

  Unity-Version:   6000.3.11f1 (Unity 6 LTS)
  Render Pipeline: Universal Render Pipeline (URP)


-------------------------------------------------------------------------------
2. SPIEL STARTEN
-------------------------------------------------------------------------------

  Fertige Version:
    LetzteSchicht.exe ausfuehren (Windows 64-bit)

  Aus dem Projekt heraus:
    1. Projekt in Unity 6000.3.11f1 oeffnen
    2. Szene Assets/Scenes/MainMenu laden
    3. Play druecken

  Kopfhoerer werden empfohlen - das Spiel arbeitet stark mit 3D-Audio.


-------------------------------------------------------------------------------
3. STEUERUNG
-------------------------------------------------------------------------------

  Taste           Aktion
  --------------  -----------------------------------------------------------
  W A S D         Bewegen
  Maus            Umsehen
  E               Benutzen / Aufheben / Untersuchen / Notiz schliessen
  R               Tuer abschliessen (nur bei geschlossener Tuer)
  F               Taschenlampe an / aus
  ESC             Pausenmenue oeffnen und schliessen

  Hinweis: In Nacht 1 werden alle Tasten im Spiel selbst erklaert
  (Hinweistexte oben links und weisse Markierungen an wichtigen Objekten).


-------------------------------------------------------------------------------
4. SPIELABLAUF
-------------------------------------------------------------------------------

  Vier Naechte, jede mit einer Aufgabenliste am Aufgabenbrett im
  Hausmeisterraum. Sind alle Aufgaben erledigt, kann die Nacht am
  Bett beendet werden.

  Aufgabenarten:
    - Strom einschalten (Sicherungskasten im Keller)
    - Tueren abschliessen
    - Raeume kontrollieren
    - Notizen lesen
    - Muell einsammeln und in den Muelleimer werfen

  Nacht 1 dient als Tutorial. Ab Nacht 2 veraendert sich das Gebaeude:
  Die Beleuchtung wird von Nacht zu Nacht schwaecher, bis in Nacht 4
  fast nur noch die Taschenlampe bleibt.


-------------------------------------------------------------------------------
5. REFERENZEN UND FREMDSOURCEN
-------------------------------------------------------------------------------

5.1 EIGENE INHALTE (selbst erstellt)
-------------------------------------------------------------------------------

  3D-Modelle (alle selbst erstellt, ausser den unter 5.3 genannten):

    - Hausmeisterhaus (Bett, Schreibtisch, Stuhl, Spind, Kisten, Mop, Eimer)
    - Moebel (Tische, Stuehle, Regale)
    - Tueren und Tuerrahmen
    - Muellobjekte und Muelleimer
    - Leuchtstoffroehren und Lampen
    - Sicherungskasten
    - Notizzettel (Modell)
    - Aussenbereich (Laternen, Wege)

  Ausserdem selbst erstellt:

    - Level-Design und kompletter Szenenaufbau
    - Beleuchtungssetup und Post-Processing
    - Alle Texte im Spiel (Notizen, Aufgaben, Hinweise)
    - Spielkonzept, Aufgabenstruktur und Dramaturgie der vier Naechte
    - Saemtliche Konfiguration der Skripte im Unity-Inspector


5.2 EXTERNE INHALTE (heruntergeladen)
-------------------------------------------------------------------------------

  TEXTUREN

    Notiz-Textur (altes Papier)
      Quelle:  https://pixabay.com/de/images/search/old%20paper/
      Lizenz:  Pixabay Content License
               (freie Nutzung, keine Namensnennung erforderlich)
      Datei:   [Dateiname im Projekt eintragen]

  SOUNDS
    Alle verwendeten Sounds stammen von Freesound.org und werden hier gemäß den jeweiligen Lizenzbedingungen ausgewiesen.
    Quelle:  https://freesound.org/

  Lizenzen
    CC0 – Creative Commons 0: https://creativecommons.org/publicdomain/zero/1.0/
    CC BY 3.0 – Attribution 3.0: https://creativecommons.org/licenses/by/3.0/
    CC BY 4.0 – Attribution 4.0: https://creativecommons.org/licenses/by/4.0/
    CC BY-NC 4.0 – Attribution NonCommercial 4.0: https://creativecommons.org/licenses/by-nc/4.0/
    Sampling+ – Sampling Plus 1.0: https://creativecommons.org/licenses/sampling+/1.0/


  ENGINE UND PAKETE

    - Unity 6 (Unity Technologies)
    - Universal Render Pipeline (Unity Technologies)
    - TextMeshPro (Unity Technologies)
    - Input System (Unity Technologies)


5.3 KI-UNTERSTUETZUNG
-------------------------------------------------------------------------------

  Für dieses Projekt wurde der KI-Assistent Claude (Anthropic) als unterstützendes Werkzeug im Entwicklungsprozess eingesetzt. Die Erstellung und Weiterentwicklung der Quelltexte erfolgte dabei in einer Kooperation zwischen dem Projektteam und der KI.

  **QUELLTEXT (alle Skripte)**

  Sämtliche C#-Skripte des Projekts wurden im iterativen Austausch zwischen uns und Claude entwickelt und weiterentwickelt. Der Ablauf war dabei grundsätzlich folgender:

  1. Eine konkrete Anforderung wurde von uns formuliert (z. B. „Türen sollen sich mit E öffnen und mit R abschließen lassen“).
  2. Claude unterstützte bei der Entwicklung des entsprechenden Codes und erläuterte dessen Funktionsweise.
  3. Der von der KI vorgeschlagene Code wurde von uns in Unity eingebaut, im Inspector konfiguriert und im Spiel getestet.
  4. Fehler, unerwünschtes Verhalten oder notwendige Änderungen wurden von uns identifiziert und an Claude zurückgemeldet.
  5. Anschließend wurde die Umsetzung gemeinsam überarbeitet und optimiert, teilweise über mehrere Iterationen hinweg.

  Auch grundlegende Architekturentscheidungen wurden nicht ausschließlich von der KI vorgegeben, sondern im Dialog zwischen Mensch und KI diskutiert und anschließend von uns bewusst ausgewählt. Dazu gehören unter anderem das ereignisbasierte Nacht-System, die interface-basierte Interaktion sowie die Trennung von `ScareEvent` und `GhostApparition`.

  Die KI diente somit als unterstützendes Werkzeug bei der Programmierung, beim Erklären von Zusammenhängen und bei der Fehlersuche. Die Integration in Unity, die Konfiguration, die Tests sowie die Beurteilung und Anpassung der vorgeschlagenen Lösungen wurden von uns durchgeführt.

  Betroffen sind alle unter Punkt 6.2 aufgeführten Skripte.


  3D-MODELLE

    - SL_Gebaeude (Hauptgebaeude der Hochschule)
    - Ghost_BASE_rigged sowie die Varianten
      Ghost_A_Flurende, Ghost_B_Hoersaal, Ghost_C_Verfolgung


  NICHT mit KI erstellt wurden:

    - Level-Design und Szenenaufbau
    - alle uebrigen 3D-Modelle (siehe 5.1)
    - Beleuchtung, Sound-Auswahl und Sound-Bearbeitung
    - Spielkonzept und Dramaturgie
    - saemtliche Inspector-Konfiguration


-------------------------------------------------------------------------------
6. HINWEISE ZUM QUELLTEXT
-------------------------------------------------------------------------------

6.1 AUFBAU
-------------------------------------------------------------------------------

  Das Projekt ist ereignisbasiert aufgebaut. Zentrale Schaltstelle ist der
  NightManager: Er löst bei jedem Nachtwechsel das statische Event
  OnNightChanged(int night) aus. Alle Systeme, die sich pro Nacht veraendern,
  melden sich selbst dort an - dadurch kennen sie einander nicht und
  koennen unabhaengig voneinander erweitert werden.

  Die Interaktion laeuft ueber drei Interfaces (IInteractable, IExaminable,
  ILockable). PlayerCast löst jedes Frame einen SphereCast aus und fragt
  ueber GetComponentInParent ab, welche dieser Interfaces das getroffene
  Objekt anbietet. Neue interagierbare Objekte brauchen deshalb nur das
  passende Interface zu implementieren.


6.2 SKRIPTUEBERSICHT
-------------------------------------------------------------------------------

  SPIELER
    CharakterController ........ Bewegung, Schwerkraft, Schrittgeraeusche
    CameraController ........... Maussteuerung, Empfindlichkeit
    PlayerCast ................. Zielerfassung und Ausloesen von [E] / [R]
    PlayerInventory ............ Schluessel und zaehlbare Gegenstaende
    Flashlight ................. Taschenlampe an / aus

  INTERAKTION (Interfaces und Weltobjekte)
    IInteractable / IExaminable / ILockable ... Interfaces
    InteractionType ............ Enum und Uebersetzung in UI-Text
    Door ....................... Tuer: oeffnen, schliessen, abschliessen
    NightDoorState ............. Stellt eine Tuer pro Nacht anders ein
                                 (offen/zu, verschlossen/frei)
    KeyItem .................... Schluessel zum Aufheben
    FlashlightPickup ........... Taschenlampe zum Aufheben
    NoteItem ................... Lesbare Notiz mit Aufgaben-Anbindung
    FuseBox .................... Sicherungskasten, schaltet Lichter
    BreakableWindow ............ Fenster zerbricht einmalig beim Betreten der
                                 Zone
    TrashItem / TrashBin ....... Muell aufheben und wegwerfen

  NACHT-SYSTEM
    NightManager ............... Verwaltet die aktuelle Nacht, Events
    NightEndObject ............. Beendet die Nacht (Blende, Uebergang)
    NightDependentObject ....... Blendet Objekte je nach Nacht ein/aus
    NightLightState ............ Stellt eine Lampe pro Nacht anders ein
    ScreenFader ................ Schwarzblende
    NightSkip .................. Nur zum Testen: Taste N

  AUFGABEN
    ObjectiveManager ........... Aufgabenlisten pro Nacht, Aufgabenbrett
    ObjectiveZone .............. Aufgabe durch Betreten erfuellen
    ObjectiveProgressTrigger ... Loest Ereignisse bei Aufgabenfortschritt aus
    GhostSequenceTrigger ....... Feuert, wenn alle Aufgaben ausser Muell
                                 erledigt sind -> startet Geist-Sequenz

  SOUND
    SoundUtils ................. Zentrale Hilfsklasse fuers Abspielen
    MusicPlayer ................ Hintergrundmusik mit weichen Uebergaengen
    SoundOcclusion ............. Daempft Sounds hinter Waenden (Raycast)
    GameSettings ............... Lautstaerken und Maussensitivitaet (statisch,
                                 ueberlebt Szenenwechsel)

  HORROR-EREIGNISSE
    ScareEvent ................. Baustein: Musik, Sound, Licht, Objekte
    GhostApparition ............ Erscheinende Gestalt mit Sichtpruefung
    PhantomSound ............... Geraeusch aus unmoeglicher Richtung
    FollowingFootsteps ......... Schritte, die dem Spieler folgen
    FlickeringLight ............ Flackernde Lampe in vier Modi
    GhostScareSequence ......... Grosse Geist-Sequenz: Anflug, Lunge,
                                 Blackout, Aufwachen
    BasementFinale ............. Keller-Finale: nahende Schritte, Schlag, Fade
                                 to Black -> Ende

  BENUTZEROBERFLAECHE
    UIController ............... Interaktions-Hinweis, Notizfenster, Brett
    HintUI ..................... Sammelnde Tutorial-Hinweise
    TutorialHintZone ........... Zonen, die Hinweise ausloesen
    IdleHintSystem ............. Tipp bei laengerem Stillstand
    InteractHighlight .......... Weisse Markierung um Tutorial-Objekte
    PauseMenu .................. Pausenmenue (ESC)
    MainMenuController ......... Hauptmenue
    SettingsPanel .............. Einstellungs-Regler
    EndScreen .................. Endbildschirm 'THE END'-> zurueck ins Hauptmenue


===============================================================================
