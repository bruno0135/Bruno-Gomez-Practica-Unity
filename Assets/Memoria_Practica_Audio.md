# Memòria Procedimental: Disseny Sonor en Unity

**Alumne:** Bruno Gomez  
**Assignatura:** Disseny de So en Videojocs  
**Projecte:** Pràctica de Paisatge Sonor 3D  

---

## 0. Dades del Projecte
*   **Escena de treball:** `Bruno Gomez Stirparo Practica`
*   **Ruta de l'escena:** `Assets/Bruno Gomez Stirparo Practica.unity`
*   **Carpeta d'àudios:** `Assets/_Creepy_Cat/_3D Scifi Kit Starter Kit_HD/_Sounds/`

---

## 1. Introducció
Aquesta memòria detalla la implementació tècnica del paisatge sonor per a l'escena "Bruno Gomez Stirparo Practica". L'objectiu ha estat crear una experiència immersiva utilitzant les eines avançades d'àudio d'Unity, complint amb els criteris de complexitat, optimització i interactivitat.

---

## 2. Configuració d'Emissors 3D i Mixers
S'ha implementat una jerarquia de mescla professional utilitzant un **Audio Mixer** centralitzat.

*   **Mixers i Grups**: S'ha creat el `MainMixer` amb subgrups per a **SFX**, **Musica** i **Ambiente**. Tots els Audio Sources de l'escena s'han encaminat (routing) a aquests grups per permetre un control dinàmic del volum i efectes.
*   **Atribució 3D**: Els emissors de l'alarma s'han configurat amb `Spatial Blend` al 100% (3D), utilitzant una corba de degradació logarítmica per simular la pèrdua de pressió sonora amb la distància.

---

## 3. Oclusió Sonora (Raycasting)
Per complir amb el criteri d'oclusió, s'ha desenvolupat un sistema basat en **Raycasting**.
*   **Funcionament**: El script `SoundOcclusion` llança un raig des de l'emissor (alarma) cap a l'oient (jugador).
*   **Efecte**: Si un obstacle bloqueja la línia de visió, s'activa automàticament un **Low Pass Filter**, reduint les freqüències altes i el volum per simular la paret física.

---

## 4. Sons Sincronitzats i Triggers
*   **Keyframes**: Mitjançant el script `AnimationSoundSync`, s'han vinculat clips de so a moments exactes de les animacions del personatge, assegurant que el so dels passos o accions coincideixi amb el moviment visual.
*   **Triggers d'Ambient**: S'han utilitzat **Box Colliders** en mode Trigger per detectar l'entrada del jugador a zones específiques (com zones interiors), disparant transicions de snapshots del Mixer per canviar l'atmosfera sonora.

---

## 5. Música Interactiva i Capes
S'ha implementat un sistema de **Música Vertical**:
*   S'utilitza el `VerticalMusicManager` per reproduir múltiples capes de música (Base, Tensió) de forma sincronitzada.
*   El volum de cada capa canvia dinàmicament segons l'estat del joc o la posició del jugador, permetent transicions suaus sense talls en el ritme.

---

## 6. Sons de Col·lisió i Superfícies (Randomització)
El controlador del jugador s'ha millorar per suportar **multi-superfícies**:
*   El sistema detecta el **Tag** del terra (Metal, Terra, Fusta).
*   Selecciona aleatòriament un clip d'un array de sons per evitar la repetitivitat (Random Pitch/Clip), millorant la qualitat sonora general.

---

## 7. Optimització
Tots els Audio Clips s'han configurat segons la seva mida i ús:
*   **Sons curts**: Configurats com *Decompress On Load* per a una resposta immediata.
*   **Música i Ambient**: Configurats com *Streaming* per estalviar memòria RAM.

---

## 8. Apèndix: Scripts Utilitzats
Per a la realització d'aquesta pràctica s'han desenvolupat i configurat els següents scripts personalitzats:

*   **`SimplePlayerController.cs`**: Gestiona el moviment del personatge i el sistema de passos. Inclou un sistema de Raycasting cap a terra per detectar el **Tag** de la superfície i reproduir el conjunt de sons (Metal, Fusta, etc.) corresponent.
*   **`AutomaticDoor.cs`**: Controla l'obertura i tancament de les portes automàtiques. Gestiona la reproducció dels clips de so d'obertura/tancament sincronitzats amb el moviment físic i permet encaminar l'àudio al canal SFX del Mixer.
*   **`TimedSoundTrigger.cs`**: Controla les alarmes i sons periòdics. Permet configurar un `delay`, el volum mestre i la repetició automàtica cada X segons, forçant una configuració 3D logarítmica.
*   **`SoundOcclusion.cs`**: Implementa l'oclusió física. Calcula mitjançant `Physics.RaycastAll` si hi ha obstacles entre l'emissor i el receptor, aplicant un filtre passa-baix (`AudioLowPassFilter`) en temps real.
*   **`VerticalMusicManager.cs`**: Sistema de música interactiva que reprodueix múltiples capes sincronitzades. Permet fer crossfades de volum entre capes per canviar la intensitat de la música.
*   **`AudioZoneTrigger.cs`**: Detecta quan el jugador entra en un volum (Trigger) i dispara transicions de **Snapshots** del Mixer per canviar la configuració de so d'interior a exterior.
*   **`AnimationSoundSync.cs`**: Monitoritza l'estat de l'Animator i dispara clips de so en moments precisos (normalized time) de les animacions, ideal per a keyframes de so.
*   **`AmbienceManager.cs`**: Centralitza el control dels bucles ambientals globals i la seva connexió amb els grups del Mixer.

---

## 9. Conclusions
La pràctica demostra un ús avançat de l'àudio en Unity, passant d'un so pla a un entorn reactiu, oclús i espacialment coherent que compleix amb tots els objectius establerts.
