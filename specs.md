Das die Dinge, die noch erledigt werden müsssen

# Test-Button
- dieser button soll einen befehl an das jeweilige entwicklerboard senden und bei einer antwort vom jeweiligen board, 
eine info in der jeweiligen textbox ausgeben, das der test erfolgreich war, das funktioniert leider nicht

# fehlende leerzeichen in der textbox ausgabe 
- manchmal fehlen leerzeichen in der ausgabe der textbox für die rollenzentrierung und der schneidmaschine oder das leerzeichen wird an der falschen stelle gesetzt
z.b.:
- richtig "ESP32 antwortet>> !!! Sensor2 5x getriggert, bewegenach links!!!"
- falsch "ESP32 antwortet>> !!! Sensor 25x getriggert, bewegenach links !!!"
kannst du das fixen?

# footer
der footer soll ganz nach unter an den unteren fensterrand und wenn man das fenster resized, dann soll der footer immer am unteren fensterrand bleiben

# GroupBoxen sollen mit fenster mit wachsen
oben die GroupBoxen sollen mit dem fenster mit wachsen und sie sollen nicht mit dem geschlossenem slider-button überlappen

# Status der verbindung im Slider
der Status vom TextBlock
"<TextBlock HorizontalAlignment="Right" VerticalAlignment="Center" Margin="0,0,10,0" Width="150" FontSize="20">Status:</TextBlock>"
und
"<TextBlock HorizontalAlignment="Right" VerticalAlignment="Center" Margin="0,0,10,0" Width="150" FontSize="20">Status:</TextBlock>"
im slider wird nicht richtig aktualisiert, manchmal steht im status "disconnected" obwohl connected ist, unten im footer, wird der status richtig angezeigt,
kannst du die funktion vom footer auch für die textblöcke verwenden

# Darstellung der Buttons
im "<Frame x:Name="Main"" ist die darstellung der buttons etwas verrutscht, die anderen buttons müssen weiter nach unten, weil links der home-button und die weiteren buttons, 
die noch angezeigt werden könnten, unter dem text "40er Streifen" liegt

# Keybinding für die Buttons
es soll ein fenster entwickelt werden, wo ich angeben kann, mit welcher taste ich einen button betätigen kann, das soll gespeichert werden, 
damit beim neustart der app, die einstellungen gleich sind.
die ausgewählten tasten, sollen mit in den button-text geschrieben werden
die buttons die für das keybinding genutzt werden sollen, sind die buttons aus dem "Einzel-Schritt", "Halb-Automatik" und "Automatik"

# ComboBox Ports namen vom entwicklerboard
kann man irgendwie ermitteln, um was für ein board es sich handelt und kann man das dann in der liste der comboboxen in klammern hinter den com-port schreiben, 
da sich die com-ports immer ändern und ich mehrere geräte habe, die den com-port nutzen

## ComboBox Ports namen vom entwicklerboard - weitere anpassungen
- es soll noch eine sicherheit eingebaut werden, wenn das board identifiziert wurde und es nicht mit der GroupBox übereinstimmt, z.b. 
ich stelle in der groupbox "Verbindung mit Schneidmaschine" her und es wird aber das board für die Rollenzentrierung erkannt, 
soll eine messagebox auf gehen und mich informieren, das es sich um das falsche board handelt,
diese sicherheit soll für beide groupboxen implementiert werden
- ich habe gesehen, wenn das board von dem einen com-port erkannt wurde und das andere noch nicht, das der com-port, der nicht erkannt wurde,
den namen aus dem geräte-manager bekommt z.b. "Silicon Labs CP210x USB to UART Bridge", das könnte man doch schon hinzufügen, wenn die com-ports in die combobox-liste hinzugefügt werden,
so das man schon sehen kann, das es sich um eins der board handeln muss und dann, wenn eine verbindung zu dem board hergestellt wurde und 
die identität ermittelt wurde, das dann dieser name aus dem geräte-manager durch den namen, vom iodentifizierten board ersetzt wird
