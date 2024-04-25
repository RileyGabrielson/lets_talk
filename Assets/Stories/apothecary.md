# Apothecary Intro

## The Apothecary

`IF visited_apothecary == 0`
[[apothecary#First Time Apothecary]]

[[apothecary#Apothecary]]

# First Time Apothecary

`SET visited_apothecary = 1`

A thin, red haired woman sits at a lab table in the middle of the tent, tweaking dozens of flames and glass vials filled with colorful liquids.

- [[apothecary#Startle Her|"Excuse me?"]]
- [[apothecary#Wait|Wait for her to see you]]

# Startle Her

At the sound of your voice the apothecary jumps, knocking a purple vial over on to the table, which promptly begins to bubble and hiss.

Cursing, the woman begins trying to mop up the liquid with a rag. The rag also begins to bubble and hiss.

"Shade extract is not worth the trouble" she mutters under her breath. Leaving the noxious mess behind, she finally looks up at you.

[[apothecary#Apothecary]]

# Wait

You patiently wait while the woman goes about her work. After a few minutes of carefully mixing vials, she pours a single drop of inky black liquid into a pill capsule.

"Oh you beauty..." she says, bringing the pill up to the light and admiring it. 

Finally, she notices she is not alone in the tent.

[[apothecary#Apothecary]]

# Apothecary

- [[apothecary#Buy Health Potion|Buy Health Potion]]
- [[town#Camp|Leave]]

# Buy Health Potion

`SET money = money - 10`
`SET num_health_potions = num_health_potions + 1`

She hands you a small vial filled with red liquid. "This should heal you up in a pinch!"

`IF num_health_potions != 1`
You now have `$num_health_potions` health potions.

`IF num_health_potions == 1`
You now have 1 health potion.

[[apothecary#Apothecary]]