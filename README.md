# Survival Top-down

Prototype game survival góc nhìn top-down làm bằng Unity. Người chơi di chuyển bằng joystick ảo, chiến đấu qua 5 wave gồm quái cận chiến và quái đánh xa, nhận EXP và tăng chỉ số khi lên cấp.

## Unity version

- Unity `6000.3.10f1` (Unity 6.3 LTS)
- Universal Render Pipeline `17.3.0`
- Input System `1.18.0`
- Cinemachine `3.1.7`
- AI Navigation `2.0.10`

Nên mở project bằng đúng phiên bản Unity trên để tránh Unity tự nâng cấp asset, scene hoặc package.

## Cách mở và chạy

1. Clone repository và thêm thư mục project vào Unity Hub bằng **Add project from disk**.
2. Mở project bằng Unity `6000.3.10f1` và chờ Package Manager import xong toàn bộ package.
3. Nếu model, material hoặc animation bị thiếu, reimport các dependency ở mục [Asset dependencies](#asset-dependencies).
4. Mở scene chính: `Assets/Scenes/SampleScene.unity`.
5. Nhấn **Play**. Wave đầu tiên tự bắt đầu sau 2 giây.

`SampleScene` đã được thêm và bật trong **Build Profiles > Scene List**.

## Điều khiển

### Mobile / chuột trong Editor

| Điều khiển | Chức năng |
|---|---|
| Joystick bên trái | Di chuyển và đổi hướng Player |
| Nút lớn bên phải | Đánh thường: bắn 3 viên theo góc `-15°, 0°, +15°` |
| Nút Blast | Đặt bom tại vị trí Player |
| Nút Repulse | Dash theo hướng forward rồi gây nổ |
| Play Again | Chơi lại sau Victory hoặc Game Over |

Trong Editor có thể kéo joystick và bấm các nút UI bằng chuột.

### Gamepad

| Input | Chức năng |
|---|---|
| Left Stick | Di chuyển |
| Button South | Đánh thường |
| Button West | Blast / đặt bom |
| Button North | Repulse / dash |

Project hiện chưa cấu hình phím bàn phím riêng; luồng test chính dùng UI hoặc gamepad.

## Phần đã làm

### Player và combat

- Player có `500 HP`, tốc độ di chuyển `2 unit/s`, tốc độ xoay `180°/s`, giáp ban đầu `0`.
- Camera top-down follow Player bằng Cinemachine.
- Sát thương Player nhận: `max(0, sát thương gốc - giáp)`.
- Sát thương Player gây ra: `sát thương gốc x (1 + Damage Multiplier)`.
- Hướng bắn và dash lấy theo `forward` hiện tại của Player.
- Đánh thường bắn 3 projectile theo góc `-15°, 0°, +15°`, mỗi viên có damage gốc `10`.
- Tối đa 3 charge, hồi 1 charge mỗi 3 giây, fire interval 0.5 giây.
- Bom nổ sau 2 giây, damage gốc 50, bán kính 5 unit, cooldown 12 giây.
- Dash 3 unit trong 0.5 giây, nổ damage gốc 15 trong bán kính 3 unit, cooldown 6 giây.
- Projectile chỉ xử lý các layer mục tiêu được cấu hình và được tái sử dụng bằng Object Pool.

### Enemy

- Quái cận chiến: `220 HP`, tốc độ `3`, tầm đánh `1.3`, damage gốc `30`.
- Quái đánh xa: `180 HP`, tốc độ `2.7`, tầm bắn `3`, dùng đạn độc.
- Đạn độc có tốc độ `10 unit/s`, tồn tại `0.5 giây` tương ứng tối đa 5 unit.
- Độc tick ngay khi trúng và thêm 3 tick, mỗi tick cách 1 giây; dính lại sẽ refresh thời gian và không cộng dồn coroutine.
- Enemy tìm đường bằng NavMesh, dừng để tấn công, phát animation Idle/Walk/Attack/Die/Victory.
- Enemy có health bar world-space luôn hướng về camera và damage popup khi nhận sát thương.
- Enemy được spawn, thu hồi và tái sử dụng bằng Object Pool.

### Wave, EXP và kết quả

- Có 5 `WaveData`; wave kế tiếp chỉ bắt đầu khi toàn bộ enemy của wave hiện tại đã chết.
- Wave 1-4 spawn ngẫu nhiên 3-4 melee và 1-2 ranged.
- Mỗi enemy cho 30 EXP; đủ 100 EXP tăng 1 level và giữ EXP dư.
- Mỗi level tăng 40 HP hiện tại, 40 Max HP, 2 giáp và 0.1 Damage Multiplier.
- Có UI hiển thị wave hiện tại, tiến độ wave, số enemy còn sống, EXP và level.
- Có màn hình Victory/Game Over và nút Play Again để reload scene.

### UI và bonus

- Player health bar, level, EXP bar, joystick và ba nút kỹ năng.
- UI charge cho đánh thường và cooldown cho bom/dash.
- Attack range indicator cho đánh thường, vùng nổ bom và dash.
- Camera shake khi projectile của Player trúng enemy, bom nổ và Player nhận damage.
- VFX spawn/hit/bomb/dash, SFX walking/hit/bomb/level up/victory/lose/button và nhạc nền.
- Object Pool cho enemy, projectile, VFX và damage popup.
- Thông số gameplay chính được tách thành ScriptableObject trong `Assets/Data`.

## Phần chưa làm / chưa khớp hoàn toàn với đề

- Đòn melee hiện kiểm tra khoảng cách và quay thẳng về Player trước khi gây damage, nhưng chưa có phép kiểm tra góc hình nón `50°` riêng tại damage frame.
- `Wave5.asset` hiện được tăng độ khó thành 4-5 melee và 2-3 ranged; đề yêu cầu mỗi wave đều là 3-4 melee và 1-2 ranged.
- Chưa có binding bàn phím; đây không phải yêu cầu bắt buộc vì project dùng joystick ảo.
- Video gameplay và file build Android/Windows không được lưu trong repository này.

## Cấu hình gameplay

Các chỉ số có thể chỉnh trực tiếp trong ScriptableObject:

| Dữ liệu | Đường dẫn |
|---|---|
| Đánh thường | `Assets/Data/Attacks/BasicAttack.asset` |
| Bom | `Assets/Data/Skills/Bomb.asset` |
| Dash | `Assets/Data/Skills/Dash.asset` |
| Player progression | `Assets/Data/Progression/PlayerProgression.asset` |
| Enemy melee/ranged | `Assets/Data/Enemies` |
| Wave 1-5 | `Assets/Data/Wave` |

## Asset dependencies

Một số raw Asset Store content không được commit vào repository. Trước khi Play, hãy mở **Window > Package Manager > My Assets** và reimport các package tương ứng:

- Tiny Bird Duo PBR Polyart
- Elementary Dungeon Pack Lite
- Golem character/animation package dùng bởi `Enemy.prefab` và `RangedEnemy.prefab`

Project vẫn lưu scene, gameplay scripts, prefab cấu hình, material tùy chỉnh, Input Actions, NavMesh data và Animator Controller do project tạo. Nếu dependency chưa được reimport, Unity có thể hiển thị model/material bị thiếu dù code gameplay vẫn compile.

## Kiểm tra nhanh trước khi nộp

1. Mở `Assets/Scenes/SampleScene.unity` và xác nhận Console không có compile error.
2. Test joystick, ba nút kỹ năng, charge và cooldown.
3. Test melee attack, poison 4 tick và refresh poison.
4. Clear từng wave, kiểm tra enemy alive count và chỉ chuyển wave khi số lượng về 0.
5. Kiểm tra EXP dư, level up và các chỉ số HP/Armor/Damage Multiplier.
6. Test cả Victory, Game Over và Play Again.
7. Build thiết bị đích từ **File > Build Profiles** sau khi scene chính đã được tick.
