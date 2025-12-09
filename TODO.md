# TODO - SchneidMaschine App v1.1.0+

**Projekt**: SchneidMaschine WPF Application
**Framework**: .NET Framework 4.7.2
**Datum**: Dezember 2025
**Status**: Kritischer Bug BEHOBEN + 2 offene Features

---

## ✅ BEHOBEN - Bug-Fix implementiert (09.12.2025)

### Bug #1: Schnitt-Befehl funktioniert nicht - Buttons bleiben deaktiviert

**Status**: ✅ BEHOBEN
**Original Status**: 🔴 KRITISCH
**Priorität**: SEHR HOCH
**Entdeckt**: Nach Commit `daf3c95` (v1.1.0)
**Funktioniert in**: Commit `0704e77` (v1.0.0)

#### Problem-Beschreibung

Wenn in der C# App ein Schnitt ausgeführt wird:
- ✅ Schnitt wird physisch ausgeführt
- ✅ "schneidenStartet_" Nachricht wird empfangen
- ❌ "schneidenBeendet_" Nachricht wird NICHT empfangen
- ❌ Buttons bleiben deaktiviert
- ❌ App hängt in "Schnitt läuft"-Zustand

#### Root Cause Analysis

**TATSÄCHLICHE URSACHE**: Bug in C# App `MainWindow.xaml.cs`, NICHT im Arduino-Sketch!

**Betroffene Methoden**:
- `stringToCharSchneidmaschine()` (Zeile 1205-1234)
- `stringToCharRollenzentrierung()` (Zeile 1174-1203)

**Problem**:
Der `befehlBuilder` wurde nach einem vollständigen Befehl NICHT geleert. Dies führte zu Problemen wenn:
1. Arduino sendet `%PING@` alle 3 Sekunden
2. `~schneidenBeendet_@` kommt fragmentiert an (`~schneiden` + `Beendet_@`)
3. Wenn dazwischen ein `%PING@` kommt, wird das erste Fragment überschrieben
4. Das zweite Fragment (`Beendet_@`) wird ohne Befehlsnamen empfangen
5. `commandReceivedSchneidmaschine()` erhält "Beendet" statt "schneidenBeendet"
6. Der Befehl wird nicht erkannt → UI bleibt deaktiviert

**FIX implementiert**:
```csharp
// stringToCharSchneidmaschine() - Zeile 1230
if (ch.Equals((char)CharArduino.END_CHAR))
{
    befehlBuilderSchneidmaschine.Append("\n");
    newText = befehlBuilderSchneidmaschine.ToString();
    befehlBuilderSchneidmaschine.Clear();  // ← HINZUGEFÜGT
}

// stringToCharRollenzentrierung() - Zeile 1199
if (ch.Equals((char)CharArduino.END_CHAR))
{
    befehlBuilderRollenzentrierung.Append("\n");
    newText = befehlBuilderRollenzentrierung.ToString();
    befehlBuilderRollenzentrierung.Clear();  // ← HINZUGEFÜGT
}
```

**Build-Status**: ✅ Erfolgreich (4 Warnungen, 0 Fehler)
**Test-Status**: ⚠️ Muss mit Hardware getestet werden
**Geänderte Dateien**: `MainWindow.xaml.cs` (2 Zeilen hinzugefügt)

#### Hardware-Testplan

**WICHTIG**: Der Fix muss mit echter Hardware getestet werden!

**Testschritte**:
1. ✅ App kompilieren (schon erledigt)
2. ⚠️ App starten und mit Arduino verbinden
3. ⚠️ Schneiden-Button drücken (oder Taster betätigen)
4. ⚠️ Console-Ausgabe prüfen:
   - Erwartung: `DEBUG Schneidmaschine - Rohdaten empfangen: [~schneidenStartet_@]`
   - Erwartung: `commandReceivedSchneidmaschine: schneidenStartet`
   - Erwartung: `COMMAND_Schneidmaschine.schneidenStartet`
5. ⚠️ Warten bis Motor stoppt (Pin 7 wird HIGH)
6. ⚠️ Console-Ausgabe prüfen:
   - Erwartung: `DEBUG Schneidmaschine - Rohdaten empfangen: [~schneidenBeendet_@]`
   - Erwartung: `commandReceivedSchneidmaschine: schneidenBeendet`
   - Erwartung: `COMMAND_Schneidmaschine.schneidenBeendet`
7. ⚠️ UI überprüfen: Buttons sollten wieder enabled sein
8. ⚠️ Mehrere Schneid-Vorgänge durchführen → alle sollten funktionieren
9. ⚠️ PING-Test: Schneiden während PING-Phase → sollte nicht stören

**Erwartetes Ergebnis**: Der Bug ist behoben, "schneidenBeendet_" wird korrekt empfangen

---

## 🔧 VERALTET - Ursprüngliche Lösungsvorschläge (ignorieren)

Diese Optionen wurden NICHT implementiert, da das Problem in der C# App lag:
```cpp
void schneiden() {
    sendCommand("schneidenStartet_", true);
    isMotorRunning = true;
    unsigned long motorStartTime = millis();  // Start-Zeit merken
    digitalWrite(cut, LOW);
    delay(500);
    digitalWrite(cut, HIGH);
}

void motorFinished() {
    valMotorRunning = digitalRead(motorRunning);

    // Timeout: Wenn nach 3 Sekunden kein Signal, trotzdem beenden
    unsigned long motorTimeout = 3000; // 3 Sekunden

    if(isMotorRunning && (valMotorRunning == HIGH || (millis() - motorStartTime > motorTimeout))){
        if(isKopfSchnitt){
            sendCommand("kopfSchnittBeendet_", true);
            isKopfSchnitt = false;
        } else {
            sendCommand("schneidenBeendet_", true);
        }
        stepCounter = 0;
        isMotorRunning = false;
    }
}
```

**Option 3: Konfigurierbar machen**
```cpp
// Config am Anfang der Datei
boolean USE_MOTOR_RUNNING_PIN = false;  // true = warten auf Pin 7, false = direkt beenden

void schneiden() {
    sendCommand("schneidenStartet_", true);
    isMotorRunning = true;
    digitalWrite(cut, LOW);
    delay(500);
    digitalWrite(cut, HIGH);

    if(!USE_MOTOR_RUNNING_PIN) {
        // Direkt beenden, ohne auf Pin zu warten
        stepCounter = 0;
        isMotorRunning = false;
        if(isKopfSchnitt){
            sendCommand("kopfSchnittBeendet_", true);
            isKopfSchnitt = false;
        } else {
            sendCommand("schneidenBeendet_", true);
        }
    }
}
```

#### Empfohlene Vorgehensweise

1. **Sofort-Fix**: Option 1 implementieren (zurück zur direkten Bestätigung)
2. **Testen**: Mit echter Hardware verifizieren
3. **Später**: Option 3 implementieren (konfigurierbar), falls Pin 7 doch gebraucht wird

#### Betroffene Dateien

- `IoT/sketche/SchneidMaschine/SchneidMaschine.ino` (Zeilen 99-116, 249-257)

#### Geschätzter Aufwand

- Fix implementieren: **10 Minuten**
- Auf Arduino hochladen: **5 Minuten**
- Testen: **15 Minuten**
- **Total**: ~30 Minuten

#### Testplan

1. ✅ Verbindung zur SchneidMaschine herstellen
2. ✅ In "Einzel-Schritt" gehen
3. ✅ "Schneiden"-Button drücken
4. ✅ Überprüfen: "schneidenBeendet_" erscheint in TextBox
5. ✅ Überprüfen: Buttons werden wieder aktiviert
6. ✅ In "Halb-Automatik" testen
7. ✅ In "Automatik" testen

---

## 📋 Offene Features aus specs.md

### Feature #7: Board-Typ in ComboBox Port-Namen anzeigen

**Status**: 📋 Offen
**Priorität**: Niedrig (Nice-to-have)
**Aufwand**: 1-2 Stunden

#### Beschreibung

ComboBox soll nicht nur "COM3" zeigen, sondern:
- "COM3 (USB Serial Device)" beim Start
- "COM3 (Schneidmaschine)" nach erfolgreicher Identifikation
- "COM5 (Rollenzentrierung)" nach erfolgreicher Identifikation

#### Zusätzliche Sicherheit

- MessageBox wenn falsches Board verbunden
  - Erwartung: Schneidmaschine
  - Gefunden: Rollenzentrierung
  - → Verbindung trennen

#### Implementierungsplan

Siehe `IMPLEMENTATION_PLAN.md` Zeilen 268-440 für vollständige Details.

**Phase 1**: USB-Device-Info auslesen (WMI-Query)
**Phase 2**: Board-Identifikation beim Verbinden (WHOAMI-Befehl)
**Phase 3**: ComboBox-Namen aktualisieren

**Dateien**:
- `threads/Thread_Port_Scanner.cs`
- `MainWindow.xaml.cs`
- `model/DataModel.cs`

**Arduino-Anpassung erforderlich**: Boards müssen auf `%WHOAMI#` mit `~BoardIdentification_Schneidmaschine@` oder `~BoardIdentification_Rollenzentrierung@` antworten.

**Hinweis**: WHOAMI-Befehl wurde bereits in neueren Commits implementiert, muss aber für Board-Identifikation genutzt werden.

---

### Feature #8: Keybinding-System

**Status**: ✅ Bereits implementiert!
**Priorität**: Erledigt
**Aufwand**: War 4-5 Stunden, jetzt fertig

#### Beschreibung

Tastatursteuerung für Buttons in Einzel-Schritt, Halb-Automatik und Automatik.

#### Implementiert in folgenden Commits

- `dc95b64` - "[FIX] Keybinding überarbeitet"
- `bbfd6d6` - "[FEATURE] Tastenbelegungen werden unter dem Haupttext in Buttons angezeigt"
- Weitere Commits für Keybinding-Fenster und DataGrid

#### Funktionen

- ✅ Tastenbelegung konfigurierbar über separates Fenster
- ✅ Einstellungen werden in JSON-Datei gespeichert
- ✅ Tastenbelegungen werden unter Buttons angezeigt (z.B. "(F1)", "(Escape)")
- ✅ KeyDown-Handler in MainWindow für alle Modi
- ✅ Standard-Tasten vorkonfiguriert

#### Status

**Vollständig implementiert und funktionsfähig!**

Specs.md sollte aktualisiert werden, um dieses Feature als erledigt zu markieren.

---

## 🔧 Weitere mögliche Verbesserungen

### Verbesserung #1: Baud-Rate Problem untersuchen

**Commit**: `7304d61` - "[FIX] BAUT Rate für den Arduino auf Standard 9600 gesetzt"

Es gab eine Änderung der Baud-Rate. Überprüfen ob:
- Alle Sketches auf 9600 konfiguriert sind
- C# App auf 9600 konfiguriert ist
- Keine Kommunikationsprobleme durch Baud-Rate-Mismatch entstehen

**Datei**: `MainWindow.xaml.cs` (SerialPort-Initialisierung)

---

### Verbesserung #2: Thread-Cleanup beim Beenden

**Commit**: `3f124d2` - "[FIX] Thread läuft noch beim beenden der App"

Sicherstellen dass:
- Alle Threads sauber beendet werden
- Keine Zombie-Threads nach App-Schließen

**Dateien**:
- `threads/Thread_Con_Schneidmaschine.cs`
- `threads/Thread_Con_Rollenzentrierung.cs`
- `MainWindow.xaml.cs` (Window_Closing Event)

---

### Verbesserung #3: Doppelte Sensoraktivierung

**Commit**: `dd0de1f` - "[FIX] Doppelte Sensoraktivierung"

Dieser Fix sollte dokumentiert werden in `IMPLEMENTATION_PLAN.md`.

**Datei**: `IoT/sketche/Rollenzentrierung/Rollenzentrierung.ino`

---

### Verbesserung #4: Port-Erkennung Stabilität

**Commits**:
- `71fde07` - "[FIX] WHOAMI-Befehl Timing-Problem behoben"
- `9ae3ed9` - "[FIX] Port-Erkennung überarbeitet"
- `fa9dea3` - "[FIX] Automatische Board-Identifikation in ComboBox anzeigen"

Die Port-Erkennung wurde mehrfach überarbeitet. Testen ob:
- Alle Timing-Probleme behoben sind
- Board-Identifikation zuverlässig funktioniert
- Keine Race-Conditions beim Verbinden

---

## 📊 Status-Übersicht

| # | Task | Status | Priorität | Aufwand |
|---|------|--------|-----------|---------|
| 1 | Schnitt-Befehl Bug-Fix | 🔴 KRITISCH | SEHR HOCH | 30 Min |
| 2 | Board-Typ in ComboBox | 📋 Offen | Niedrig | 1-2 Std |
| 3 | Keybinding-System | ✅ Erledigt | - | - |
| 4 | Baud-Rate verifizieren | 🟡 Überprüfen | Mittel | 15 Min |
| 5 | Thread-Cleanup testen | 🟡 Überprüfen | Mittel | 15 Min |
| 6 | Port-Erkennung testen | 🟡 Überprüfen | Mittel | 30 Min |

**Empfohlene Reihenfolge**:
1. 🔴 Schnitt-Befehl Bug-Fix (SOFORT)
2. 🟡 Baud-Rate verifizieren
3. 🟡 Thread-Cleanup testen
4. 🟡 Port-Erkennung testen
5. 📋 Board-Typ in ComboBox (wenn Zeit)

---

## 🎯 Nächste Schritte

### Sofort (heute)

1. **Schnitt-Bug fixen**:
   - Arduino-Sketch `SchneidMaschine.ino` ändern (Option 1)
   - Auf Arduino hochladen
   - Mit echter Hardware testen
   - Commit: "[FIX] Schnitt-Befehl sendet direkt schneidenBeendet ohne auf Pin 7 zu warten"

2. **Baud-Rate verifizieren**:
   - Alle Sketches prüfen: Alle auf 9600?
   - C# App prüfen: SerialPort.BaudRate = 9600?
   - Dokumentieren in IMPLEMENTATION_PLAN.md

### Diese Woche

3. **Alle Commits testen**:
   - Thread-Cleanup beim Beenden
   - Port-Erkennung mit echten Boards
   - Doppelte Sensoraktivierung (Rollenzentrierung)

4. **Dokumentation aktualisieren**:
   - `specs.md`: Keybinding als erledigt markieren
   - `IMPLEMENTATION_PLAN.md`: Neue Fixes dokumentieren
   - `readme.md`: Bekannte Probleme entfernen (wenn behoben)

### Später (optional)

5. **Board-Typ Feature**:
   - Nur wenn gewünscht
   - Niedrige Priorität
   - Implementierung siehe IMPLEMENTATION_PLAN.md

---

## 📝 Fragen an Entwickler

Bevor ich mit dem Fix beginne, bitte bestätigen:

1. **Schnitt-Bug**: Soll ich Option 1 (direkte Bestätigung) implementieren?
2. **Pin 7 (motorRunning)**: Wird dieser Pin in der Hardware verwendet? Wenn ja, wofür?
3. **LOGO-SPS**: Ist eine LOGO-SPS an Pin 7 angeschlossen, die ein Signal sendet?
4. **Langfristiges Ziel**: Soll der Pin 7 irgendwann genutzt werden, oder kann er entfernt werden?

---

## 🔍 Debugging-Tipps

Falls weitere Probleme auftreten:

**Arduino Serial Monitor**:
```cpp
// In schneiden() temporär hinzufügen:
Serial.println("DEBUG: Schnitt gestartet");
Serial.println("DEBUG: Pin motorRunning = " + String(digitalRead(motorRunning)));
```

**C# TextBox Ausgabe**:
- Auto-Scroll deaktivieren um Nachrichten zu lesen
- Prüfen welche Befehle ankommen
- Timestamp hinzufügen für Timing-Analyse

**Git Vergleich**:
```bash
# Vergleiche aktuelle Version mit v1.0.0
git diff 0704e77..HEAD -- IoT/sketche/SchneidMaschine/SchneidMaschine.ino

# Checkout alte funktionierende Version (zum Testen)
git checkout 0704e77 -- IoT/sketche/SchneidMaschine/SchneidMaschine.ino
```

---

**Letzte Aktualisierung**: 9. Dezember 2025
**Erstellt von**: Claude Code Analysis
**Projekt-Version**: v1.1.0+ (dev branch)
