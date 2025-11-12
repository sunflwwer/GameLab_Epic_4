# 데스 타일 (Death Tile) 사용 가이드 💀

## 🎮 기능

플레이어가 밟으면 죽는 타일 (가시, 용암, 구덩이 등)을 만들 수 있습니다!

- ✅ **충돌 시 즉시 사망**: 플레이어가 닿으면 죽음
- ✅ **Console 로그**: "💀 플레이어 죽음" 메시지 출력
- ✅ **기존 시스템 연동**: characterHurt 스크립트와 연동
- ✅ **리스폰**: 체크포인트에서 부활
- ✅ **시각화**: Scene View에서 빨간색으로 표시

---

## 📋 설정 방법

### 방법 1: Tilemap에 적용 (가시 타일, 용암 등)

#### Step 1: 새 Tilemap 생성
```
Hierarchy:
└─ Grid
   ├─ Ground (일반 땅)
   ├─ OneWayPlatform (일방향)
   └─ DeathTiles (새로 생성!) ← 이것 추가
```

#### Step 2: 데스 타일 배치
```
1. Tile Palette에서 가시/용암 타일 선택
2. DeathTiles Tilemap에 그리기
```

#### Step 3: 컴포넌트 추가
```
DeathTiles 오브젝트 선택:

1. Tilemap Collider 2D 추가
2. DeathTile 스크립트 추가
```

#### Step 4: 설정
```
DeathTile (Script):
├─ Destroy Player: ✅ (플레이어 파괴)
├─ Respawn Delay: 1 (리스폰까지 대기)
└─ Show Debug Logs: ✅ (로그 표시)
```

---

### 방법 2: 개별 오브젝트에 적용 (개별 가시, 톱날 등)

#### Step 1: GameObject 생성
```
Hierarchy 우클릭:
└─ Create Empty
   이름: "Spike"
```

#### Step 2: 컴포넌트 추가
```
1. Sprite Renderer 추가 (모양)
2. Collider 2D 추가 (Box Collider 2D 또는 Circle Collider 2D)
3. DeathTile 스크립트 추가
```

#### Step 3: Collider 설정
```
Box Collider 2D:
├─ Is Trigger: ✅ (Trigger로 설정하면 통과하면서 닿을 때 죽음)
└─ Is Trigger: ❌ (체크 해제하면 물리 충돌)
```

---

## ⚙️ Inspector 설정

### DeathTile 컴포넌트:
```
📦 DeathTile
├─ Destroy Player: ✅ / ❌
│  └─ 체크: 플레이어 오브젝트 파괴
│  └─ 해제: characterHurt만 호출 (리스폰)
│
├─ Respawn Delay: 1
│  └─ 플레이어 파괴까지 지연 시간 (초)
│  └─ 0 = 즉시 파괴
│  └─ 1 = 1초 후 파괴 (애니메이션 시간)
│
└─ Show Debug Logs: ✅
   └─ Console에 상세 로그 출력
```

---

## 🎯 작동 방식

### OnCollisionEnter2D:
```
플레이어가 물리 충돌하면:
→ KillPlayer() 호출
→ Console: "💀 플레이어 죽음"
```

### OnTriggerEnter2D:
```
플레이어가 Trigger 영역에 들어가면:
→ KillPlayer() 호출
→ Console: "💀 플레이어 죽음"
```

### KillPlayer():
```
1. characterHurt.hurtRoutine() 호출
   → 사망 애니메이션
   → 화면 효과
   → 체크포인트로 리스폰

2. (선택) Destroy Player
   → Respawn Delay 후 파괴
```

---

## 📍 리스폰 위치

### 리스폰 메커니즘:
```
characterHurt.hurtRoutine() 호출
    ↓
respawnTime 만큼 대기 (애니메이션)
    ↓
transform.position = checkpointFlag
    ↓
마지막 체크포인트로 이동!
```

### 리스폰 위치 결정:
```
1. 체크포인트를 밟은 경우:
   → 마지막 체크포인트 위치

2. 체크포인트를 안 밟은 경우:
   → Inspector의 "Checkpoint Flag" 값
   → 기본값: (0, 0, 0) 또는 설정된 초기 위치
```

### ⚠️ 주의사항:
```
체크포인트를 설정하지 않으면:
→ (0, 0, 0)으로 리스폰될 수 있음!
→ 게임 시작 전에 반드시 설정 필요
```

---

## 🎮 체크포인트 설정 방법

### 방법 1: Inspector에서 초기 위치 설정
```
1. Hierarchy에서 플레이어 (Kit) 선택
2. Inspector → characterHurt 컴포넌트
3. Checkpoint Flag 필드에 게임 시작 위치 입력
   예: X: 0, Y: 2, Z: 0
```

### 방법 2: 체크포인트 오브젝트 배치
```
1. 게임 내 체크포인트 오브젝트 생성
2. 플레이어가 닿으면 newCheckpoint() 호출
3. 해당 위치가 리스폰 지점으로 저장됨
```

### 방법 3: 자동으로 현재 위치를 초기 체크포인트로
```csharp
// characterHurt.cs의 Start()에 추가:
void Start() {
    body = GetComponent<Rigidbody2D>();
    
    // 체크포인트가 설정 안 되어있으면 현재 위치 사용
    if (checkpointFlag == Vector3.zero) {
        checkpointFlag = transform.position;
        Debug.Log($"초기 체크포인트 설정: {checkpointFlag}");
    }
}
```

---

## 📊 Console 로그

### 정상 작동 시:
```
💀 플레이어 죽음
⚠️ 플레이어가 데스 타일에 닿았습니다!
   - 플레이어: Kit
   - 데스 타일: DeathTiles
   - 위치: (5.2, 2.1, 0.0)
   → characterHurt.hurtRoutine() 호출됨
   → 1초 후 플레이어 파괴
   💥 죽음 이펙트 재생 위치: (5.2, 2.1, 0.0)
```

---

## 💡 사용 예시

### 예시 1: 가시 타일
```
Hierarchy:
└─ Grid
   └─ Spikes (Tilemap)
      ├─ Tilemap
      ├─ Tilemap Renderer
      ├─ Tilemap Collider 2D
      └─ DeathTile ✅

설정:
- Destroy Player: ✅
- Respawn Delay: 0.5
→ 가시에 닿으면 0.5초 후 리스폰
```

### 예시 2: 용암
```
Hierarchy:
└─ Grid
   └─ Lava (Tilemap)
      ├─ Tilemap
      ├─ Tilemap Renderer
      ├─ Tilemap Collider 2D (Trigger)
      └─ DeathTile ✅

설정:
- Is Trigger: ✅
- Destroy Player: ✅
- Respawn Delay: 1
→ 용암에 닿으면 1초 후 리스폰
```

### 예시 3: 회전하는 톱날
```
Hierarchy:
└─ RotatingSaw
   ├─ Sprite Renderer
   ├─ Circle Collider 2D
   ├─ DeathTile ✅
   └─ RotationScript

설정:
- Destroy Player: ❌ (characterHurt만 사용)
- Show Debug Logs: ✅
→ 톱날에 닿으면 즉시 리스폰
```

---

## 🎨 Scene View 시각화

### Gizmo 표시:
```
빨간색 반투명 박스: 데스 타일 범위
빨간색 X 표시: 위험 표시
```

→ Scene View에서 어디가 위험한지 한눈에 확인!

---

## 🔍 플레이어 감지 방법

DeathTile은 4가지 방법으로 플레이어를 감지:

1. **characterMovement 컴포넌트**
2. **"Player" 태그**
3. **"Player" 레이어**
4. **이름에 "Player" 또는 "Kit" 포함**

→ 어떤 방법으로든 플레이어를 자동 감지!

---

## 🎯 테스트 체크리스트

### 기본 테스트:
- [ ] 데스 타일 생성
- [ ] DeathTile 스크립트 추가
- [ ] Collider 추가
- [ ] Play 모드 실행
- [ ] 플레이어로 데스 타일 밟기
- [ ] Console: "💀 플레이어 죽음" 확인 ✅
- [ ] 플레이어 리스폰 확인 ✅

### Tilemap 테스트:
- [ ] DeathTiles Tilemap 생성
- [ ] 가시 타일 배치
- [ ] Tilemap Collider 2D 추가
- [ ] DeathTile 스크립트 추가
- [ ] 테스트 - 닿으면 죽음 ✅

---

## 🆘 문제 해결

### "플레이어가 안 죽어요"
```
1. Collider 확인
   - Collider 2D가 추가되어 있는지
   - Is Trigger 설정 확인

2. 플레이어 확인
   - Player 태그가 설정되어 있는지
   - characterMovement 컴포넌트가 있는지

3. Console 확인
   - "💀 플레이어 죽음" 메시지가 나오는지
   - Show Debug Logs를 체크하고 확인
```

### "Console에 로그가 안 나와요"
```
DeathTile (Script):
└─ Show Debug Logs: ✅ 체크 확인

그래도 안 나오면:
→ DeathTile 스크립트가 제대로 추가되었는지 확인
```

### "리스폰이 안 돼요"
```
1. characterHurt 컴포넌트 확인
   - 플레이어에 characterHurt가 있는지
   - Respawn Time 설정 확인

2. Checkpoint 확인
   - 체크포인트가 설정되어 있는지
```

---

## 💡 추가 기능

### 파티클 효과 추가:
```csharp
// DeathTile.cs의 PlayDeathEffects에서:
[SerializeField] private GameObject deathParticlePrefab;

private void PlayDeathEffects(Vector3 position) {
    if (deathParticlePrefab != null) {
        Instantiate(deathParticlePrefab, position, Quaternion.identity);
    }
}
```

### 사운드 효과:
```csharp
[SerializeField] private AudioClip deathSound;
private AudioSource audioSource;

private void PlayDeathEffects(Vector3 position) {
    if (audioSource != null && deathSound != null) {
        audioSource.PlayOneShot(deathSound);
    }
}
```

---

## 🎊 완료!

### 설정 순서:
1. Tilemap 또는 GameObject 생성
2. Collider 2D 추가
3. DeathTile 스크립트 추가
4. 테스트!

### 결과:
- ✅ 플레이어가 닿으면 죽음
- ✅ Console: "💀 플레이어 죽음"
- ✅ 체크포인트로 리스폰
- ✅ Scene View에서 빨간색으로 표시

**완벽한 데스 타일!** 💀✨

---

## 📝 빠른 설정 (Tilemap)

```
1. Hierarchy에 "DeathTiles" Tilemap 생성
2. 가시/용암 타일 배치
3. Add Component:
   - Tilemap Collider 2D
   - DeathTile
4. Layer를 "Ground"로 설정
5. 테스트!
```

Unity에서 바로 사용할 수 있습니다! 🎮✨

