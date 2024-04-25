# Tavern Intro

## The Tavern

A few worn out travelers amble about the dimly lit tavern. A keen-eyed man stands behind the bar, handing out drinks and advice.

[[tavern#Tavern]]

# Tavern

- [[tavern#Buy a drink|Buy a drink]]
- [[tavern#Advice|Ask the barkeep for advice]]
- [[town#Camp|Leave]]

# Buy a drink

`SET money = money - 5`
`SET health = health + 10`

`IF health > max_health`
`SET health = max_health`

You knock down a drink, letting the coarse liquid burn down your throat. It tastes terrible.

Your health is now `$health` of `$max_health`. You have `$money` coins left.

[[tavern#Tavern]]

# Advice

"My advice is always the same" the barkeep says, a half smile spreading across his face.

"You're doing fine. Don't stress. If you die, we can share a drink".

[[tavern#Tavern]]