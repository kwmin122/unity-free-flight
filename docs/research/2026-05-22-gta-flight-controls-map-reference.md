# GTA Flight Controls And Map Reference Notes

Date: 2026-05-22

Purpose: ground the current MINgo control and map pass in observed GTA-style behavior without copying GTA assets, names, layout, logos, or proprietary art.

## Flight Controls

- GTA Wiki's GTA V controls table separates normal vehicles from aircraft:
  - Land/water vehicles: W/S are forward/backward acceleration.
  - Aircraft: W/S are aircraft throttle on/off.
  - A/D are aircraft yaw left/right.
  - Numpad keys handle roll and pitch.
- Sportskeeda's beginner PC flying guide describes the practical feel: hold W on land to move the plane forward for takeoff, use pitch input to lift, and press S after touchdown to stop.

Implementation decision:

- Do not make S a full air reverse. That reads like a car, but it is wrong for the aircraft fantasy.
- Make W feel immediate by ramping throttle rapidly toward full power while held.
- Make S feel useful by rapidly cutting throttle toward idle and applying the existing airbrake slowdown while airborne.
- Keep Space as the hard ground brake.

## Map Composition

Useful GTA-style world ingredients from the referenced map/landmark sources:

- A main airport near the southern urban edge.
- A downtown financial skyline with tall office towers.
- A western beach district with pier/boardwalk energy.
- A marina/harbor edge with boats and piers.
- Freeways and overpasses that read clearly from the air.
- Rural mountains, rivers, military/restricted areas, and large natural landmarks.

Implementation decision:

- Keep MINgo as one original map, not a GTA replica.
- Build readable silhouettes for flight: runway markings, terminal, glass towers, rooftop helipad, boardwalk, marina pier, boats, freeway overpass, palms, tree clusters, plaza sculpture.
- Treat every large shape as a navigation cue or landing temptation, not just decoration.

## Sources

- GTA Wiki, Controls for GTA V and GTA Online: https://gta.fandom.com/wiki/Controls_for_GTA_V
- Sportskeeda, Beginner's guide to flying planes in GTA 5 on PC: https://www.sportskeeda.com/gta/beginner-s-guide-flying-planes-gta-5-pc
- GTABase, GTA 5 Map & Locations Guide: https://www.gtabase.com/grand-theft-auto-v/map-locations/
- GTA V Wiki, Landmarks: https://gta5wiki.com/places/landmarks/
- Grand Theft Wiki, GTA V areas template: https://www.grandtheftwiki.com/Template%3AGtav_areas
