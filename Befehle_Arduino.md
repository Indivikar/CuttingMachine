# Befehle für den SerialMonitor vom Arduino

### Schrittmotor bewegen

- Format: stepperStart_[anzahl_steps]_[richtung]
- Start-Zeichen: %
- End-Zeichen: #


%stepperStart_127_forward# = 10mm vorwärts
%stepperStart_500_backward#  = 10mm rückwärts

### Schneiden

- Start-Zeichen: %
- End-Zeichen: #

%schneidenStart#