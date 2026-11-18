# UZBEKISTAN OPEN WORLD 3D

Toshkentdan Samarqandgacha — O‘zbekistondan ilhomlangan original ochiq 3D dunyo o‘yini.

Loyihada ikki qism bor:

| Qism | Papka | Holati |
|---|---|---|
| **Veb-versiya** (o‘ynasa bo‘ladi) | [`web/`](web/), tayyor fayllar: [`Yuklab_olish/`](Yuklab_olish/) | Brauzerda ishlaydi: PC, Android, iOS |
| **Unity versiyasi** | [`Assets/_Project/`](Assets/_Project/) | PHASE 1 (Core) — skriptlar |

## Veb-versiya

- O‘ynash: [`Yuklab_olish/UzbekistanOpenWorld3D.html`](Yuklab_olish/UzbekistanOpenWorld3D.html) faylini yuklab oling va brauzerda oching. Internetsiz ham ishlaydi.
- Reklama (video WebM / rasm PNG): [`Yuklab_olish/Reklama.html`](Yuklab_olish/Reklama.html)
- Ichida: Toshkent va Samarqand, avtomagistral, 5 ta mashina va tyuning, transport va svetoforlar, piyodalar, 5 ta syujet missiyasi va taksi buyurtmalari, uy va biznes sotib olish, kun-tun, ob-havo, minixarita va GPS, 3 til (O‘zbekcha · Русский · English), telefon uchun sensorli boshqaruv.
- Texnologiya: [three.js r128](https://threejs.org) (MIT litsenziyasi), bitta HTML fayl.

### Qayta yig‘ish

Manba fayllar: `web/game.body.html` (o‘yin), `web/ad.body.html` (reklama).

```bash
npm install three@0.128.0
node web/build.js node_modules/three/build/three.min.js
```

Natija `Yuklab_olish/` papkasiga yoziladi (three.js fayl ichiga joylanadi, shuning uchun internetsiz ishlaydi).

---

# UNITY VERSIYASI — PHASE 1 (CORE)

**Engine:** Unity 6 LTS + URP (Universal Render Pipeline).
**Nega Unity:** mid-range Android telefonlarda Unreal Engine 5 ga nisbatan yengilroq ishlaydi. Build hajmi kichikroq va bitta loyihadan PC, Android va iOS chiqadi.

Phase 1 da quyidagilar bor:
- 3D o‘yinchi (yurish, yugurish, sprint, sakrash, gravitatsiya);
- third-person kamera (devorga kirib ketmaydi, sprintda FOV kengayadi);
- protsedural 3D Toshkent hududi (5×5 kvartal, yo‘llar, binolar, gumbazlar, xiyobon, daraxtlar, tog‘lar);
- PC boshqaruvi (klaviatura, sichqoncha, gamepad);
- mobil boshqaruv (joystick, kamera zonasi, tugmalar, notch uchun SafeArea);
- 3 tilli lokalizatsiya (O‘zbekcha, Русский, English) va FPS hisoblagich.

---

## 1. O‘rnatish

1. **Unity Hub** ni o‘rnating, so‘ng **Unity 6 LTS** (6000.x) ni o‘rnating. Unga quyidagi modullarni qo‘shing:
   - Android Build Support (OpenJDK + Android SDK & NDK bilan);
   - iOS Build Support (iOS build faqat Mac + Xcode da chiqadi);
   - Windows Build Support (IL2CPP).
2. Unity Hub → **New project** → **Universal 3D** shablonini tanlang.
   - Nomi: `UzbekistanOpenWorld3D`.
   - ⚠️ Joylashuv yo‘lida kirill harflari bo‘lmasin. Masalan `C:\UnityProjects\UzbekistanOpenWorld3D` to‘g‘ri. `C:\Users\Админ\...` yo‘lida Android Gradle build ko‘pincha xato beradi.
3. Shu papkadagi `Assets/_Project` papkasini yangi loyihaning `Assets/` papkasiga to‘liq nusxalang.
4. **Edit → Project Settings → Player → Other Settings → Active Input Handling** ni `Input System Package (New)` yoki `Both` qilib qo‘ying. Unity qayta ishga tushadi.
   - Universal 3D shablonida Input System paketi bor. Agar yo‘q bo‘lsa: **Window → Package Manager → Input System → Install**.

## 2. Sahnani sozlash

1. **File → New Scene → Basic (URP)** → `Assets/_Project/Scenes/Prototype_Tashkent.unity` nomi bilan saqlang.
2. Hierarchy → **Create Empty** → nomini `_Bootstrap` qiling.
3. `_Bootstrap` ga **GameBootstrap** komponentini qo‘shing (Add Component → GameBootstrap).
4. Sahnadagi Main Camera va Directional Light ni o‘chirmasangiz ham bo‘ladi, bootstrap ularni o‘zi ishlatadi.
5. **File → Build Profiles** (yoki Build Settings) → sahnani ro‘yxatga qo‘shing.

### GameBootstrap Inspector sozlamalari

| Maydon | Qiymat | Izoh |
|---|---|---|
| Target Frame Rate | 60 | Kuchsiz telefonlar uchun 30 |
| Build Prototype World | ✔ | Toshkent greybox hududini quradi |
| World Seed | 2026 | Boshqa son boshqa shahar joylashuvini beradi |
| World Base Material | bo‘sh | Ixtiyoriy: URP/Lit material |
| Force Mobile Controls In Editor | ☐ | Editor da touch UI ni sinash uchun |

Play bosilganda bootstrap quyidagilarni avtomatik yaratadi:
- `[Managers]`: InputManager, LocalizationManager;
- `World_Tashkent_Prototype`;
- `Player`;
- `Main Camera` (ThirdPersonCamera bilan);
- `[HUD]`;
- `EventSystem`.

## 3. Fayl strukturasi

```
Assets/_Project/
├── Scripts/
│   ├── Core/          GameBootstrap.cs, InputManager.cs
│   ├── Player/        PlayerController.cs, PlayerFactory.cs, PlaceholderCharacterAnimator.cs
│   ├── Camera/        ThirdPersonCamera.cs
│   ├── World/         PrototypeWorldBuilder.cs, MaterialLibrary.cs
│   ├── Localization/  LocalizationManager.cs, LocalizedText.cs
│   └── UI/            GameHUD.cs, UIFactory.cs, SafeArea.cs, FPSCounter.cs
│       └── Mobile/    VirtualJoystick.cs, TouchLookArea.cs, MobileButton.cs
└── Resources/Localization/  uz.json, ru.json, en.json
```

Keyingi bosqichlar uchun bo‘sh papkalar yarating: `Art/Models`, `Art/Materials`, `Prefabs`, `Scenes`, `Audio`.

## 4. Tizimlar qanday bog‘langan

```
InputManager (-100)  ──> PlayerController (Update) ──> ThirdPersonCamera (LateUpdate)
     ▲                            │
     │ SetTouchMove/AddTouchLook  └─ State/Speed ──> PlaceholderCharacterAnimator, Animator
GameHUD ─ VirtualJoystick / TouchLookArea / MobileButton
LocalizationManager ── LanguageChanged ──> LocalizedText (har bir UI matn)
```

- **Phase 2 (mashinalar):** `VehicleController` ham `InputManager.Move` dan o‘qiydi. Mashinaga o‘tirganda `PlayerController.InputEnabled = false` qilinadi, kamera esa `SetTarget(vehicle)` bilan mashinaga o‘tadi. Kirish/chiqish uchun `InteractPressed` (E / X / Amal tugmasi) allaqachon tayyor.
- **Save tizimi:** pozitsiyani tiklash uchun `PlayerController.Teleport()` ishlatiladi.
- **Yangi matnlar:** kodda faqat kalit ishlatiladi, `LocalizationManager.Get("kalit")`, matnning o‘zi 3 ta JSON faylga yoziladi.

## 5. PLACEHOLDER lar (keyin almashtiriladi)

| Nima | Qayerda | Nima bilan almashtiriladi |
|---|---|---|
| Primitivlardan yasalgan qahramon | `PlayerFactory.cs` | Humanoid 3D model + Animator Controller (`Speed`, `Grounded`, `VerticalSpeed`) |
| Tebranish animatsiyasi | `PlaceholderCharacterAnimator.cs` | Haqiqiy idle/walk/run/jump animatsiyalari |
| Protsedural shahar | `PrototypeWorldBuilder.cs` | Phase 3: 3D modeldagi bino va yo‘l prefablari, LOD, streaming |
| Quyosh va atmosfera | `GameBootstrap.SetupLighting` | DayNightManager, WeatherManager |
| Til PlayerPrefs da saqlanadi | `LocalizationManager` | SaveManager |

## 6. Test qilish

**Editor (PC):**
1. Play bosing → ekranni bosing (kursor qulflanadi).
2. WASD bilan yuring. Shift — sprint (FOV kengayadi), Ctrl — sekin yurish, Space — sakrash.
3. Sichqoncha bilan kamerani aylantiring. Binoga yaqin borib kamera devorga kirib ketmasligini tekshiring.
4. Esc → yuqori o‘ngdagi til tugmasini bosing: UZ → RU → EN almashadi. Play ni qayta bosganda tanlangan til saqlangan bo‘lishi kerak.
5. Gamepad ulang: chap stick — yurish, o‘ng stick — kamera, A — sakrash.

**Editor (mobil simulyatsiya):**
- F1 tugmasi yoki **Window → General → Device Simulator** (masalan, Samsung Galaxy yoki iPhone tanlang). Notch li qurilmada tugmalar xavfsiz zonada qolishini tekshiring.

**Android telefon:**
1. Build Profiles → Android → **Switch Platform**.
2. Player Settings:
   - Scripting Backend = **IL2CPP**, Target Architectures = **ARM64**;
   - Graphics APIs = Vulkan + OpenGLES3;
   - Default Orientation = **Landscape Left**;
   - Texture Compression = **ASTC**.
3. Telefonda USB debugging ni yoqing → **Build And Run**.
4. Yuqori chap burchakdagi FPS ni kuzating. Mid-range telefonda maqsad 60 FPS (kamida barqaror 30).

**iOS:** Mac da Build → Xcode loyihasi ochiladi → Signing ni sozlang → qurilmaga Run qiling.

## 7. Debugging

| Muammo | Sabab va yechim |
|---|---|
| `The type or namespace 'InputSystem' could not be found` | Input System paketi o‘rnatilmagan → Package Manager dan o‘rnating |
| `InvalidOperationException: You are trying to read Input using UnityEngine.Input` | Sahnada eski StandaloneInputModule bor → EventSystem inspectorida **Replace with InputSystemUIInputModule** ni bosing |
| Hamma narsa pushti (magenta) rangda | Loyiha URP emas → Universal 3D shablonidan yarating yoki Graphics Settings da URP asset ni belgilang |
| Tugmalar bosilmaydi | Sahnada EventSystem yo‘q yoki ikkita → Hierarchy ni tekshiring |
| Matn o‘rnida `#ui.jump` chiqyapti | Kalit JSON da yo‘q yoki fayl `Resources/Localization/` papkasida emas |
| Kamera o‘yinchi ichiga kirib qoladi | Player `Ignore Raycast` layerida bo‘lishi kerak (bootstrap shunday qiladi) |
| Telefonda FPS past | `blocksX/blocksZ` ni 3 ga tushiring, Target Frame Rate = 30, URP asset da Shadow Distance = 40 |
| Android build Gradle xatosi | Loyiha yo‘lida kirill harflari yoki bo‘sh joy bor → loyihani `C:\UnityProjects\` ga ko‘chiring |

## 8. Keyingi qadam — PHASE 2

VehicleController, VehiclePhysics (WheelCollider yoki raycast suspension), 5 xil mashina sinfi (sedan, SUV, sport, taksi, yuk mashinasi), mashinaga kirish va chiqish, mobil haydash boshqaruvi (gaz, tormoz, qo‘l tormozi), oddiy traffic.
