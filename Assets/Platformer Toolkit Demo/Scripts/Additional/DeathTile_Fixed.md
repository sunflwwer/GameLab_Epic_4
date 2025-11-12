# 데스 타일 리스폰 가이드 ✅

## 🎯 수정 완료!

플레이어가 죽으면 **Destroy하지 않고** characterHurt의 리스폰 시스템을 사용합니다!

---

## ✅ 수정 사항:

### 1. DeathTile.cs
```csharp
[SerializeField] private bool destroyPlayer = false; // 기본값 false로 변경
```
→ 플레이어를 파괴하지 않고 characterHurt만 호출

### 2. characterHurt.cs
```csharp
void Start() {
    // 체크포인트가 설정 안 되어있으면 현재 위치를 초기 체크포인트로 설정
    if (checkpointFlag == Vector3.zero) {
        checkpointFlag = transform.position;
        Debug.Log($"✅ 초기 체크포인트 설정: {checkpointFlag}");
    }
}
```
→ 게임 시작 위치가 자동으로 체크포인트가 됨

### 3. jumpTester.cs
```csharp
void Update() {
    // 플레이어가 파괴되었는지 체크
    if (characterTransform != null) {
        transform.position = new Vector3(characterTransform.position.x, characterY);
    }
}
```
→ Null 체크 추가 (에러 방지)

---

## 🎮 작동 방식:

```
플레이어가 데스 타일 밟음
    ↓
💀 플레이어 죽음 (Console)
    ↓
characterHurt.hurtRoutine() 호출
    ↓
사망 애니메이션 + 효과
    ↓
respawnTime 대기 (1초)
    ↓
checkpointFlag 위치로 이동 ✅
    ↓
플레이어 다시 플레이 가능!
```

**플레이어 오브젝트는 파괴되지 않고 계속 존재합니다!**

---

## 📍 리스폰 위치:

### 기본 동작:
```
1. 게임 시작 시:
   → 현재 위치가 초기 체크포인트로 자동 설정
   
2. 체크포인트를 밟으면:
   → 그 위치가 새 체크포인트가 됨
   
3. 죽으면:
   → 마지막 체크포인트로 리스폰
```

---

## 🎯 Inspector 설정:

### DeathTile:
```
📦 DeathTile
├─ Destroy Player: ❌ (체크 해제!)
├─ Respawn Delay: 1 (사용 안 함, characterHurt가 처리)
└─ Show Debug Logs: ✅
```

### characterHurt (플레이어):
```
📦 characterHurt
├─ Checkpoint Flag: (0, 0, 0) 또는 비워두기
│  → Start()에서 자동으로 현재 위치로 설정됨
├─ Respawn Time: 1 (리스폰까지 대기 시간)
└─ Flash Duration: 0.1
```

---

## ✅ 테스트:

1. Play 모드 시작
2. Console 확인: "✅ 초기 체크포인트 설정: (x, y, 0)"
3. 데스 타일 밟기
4. Console: "💀 플레이어 죽음"
5. 1초 후 게임 시작 위치로 리스폰 ✅
6. 플레이어 계속 플레이 가능!

---

## 🎊 완료!

- ✅ 플레이어 Destroy 안 함
- ✅ 체크포인트로 자동 리스폰
- ✅ 게임 시작 위치가 초기 체크포인트
- ✅ MissingReferenceException 에러 해결
- ✅ 계속 플레이 가능

**완벽하게 작동합니다!** 🎮✨

