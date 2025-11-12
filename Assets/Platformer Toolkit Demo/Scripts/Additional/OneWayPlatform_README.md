# 일방향 플랫폼 (One-Way Platform) 사용 가이드 🪜

## 🎮 기능

- ✅ **아래에서 위로 통과**: 플레이어가 점프해서 올라갈 수 있음
- ✅ **위에서 밟기**: 플랫폼 위에서는 걸어다닐 수 있음
- ✅ **S키로 떨어지기**: S키 또는 아래 방향키를 누르면 아래로 떨어짐

---

## 📋 설정 방법

### 1. Tilemap에 적용하는 경우:

#### Step 1: Tilemap 오브젝트 선택
```
Hierarchy에서:
└─ Grid
   └─ Tilemap (일방향 플랫폼용)
```

#### Step 2: 컴포넌트 추가
1. **Tilemap Collider 2D** 추가
   - Inspector → Add Component
   - "Tilemap Collider 2D" 검색 후 추가

2. **Composite Collider 2D** 추가 (선택사항, 성능 향상)
   - Inspector → Add Component
   - "Composite Collider 2D" 검색 후 추가
   - Tilemap Collider 2D에서 "Used By Composite" 체크

3. **Rigidbody 2D** 추가
   - Composite Collider 2D를 사용하면 자동으로 추가됨
   - Body Type: **Static** 설정

4. **OneWayPlatform 스크립트** 추가
   - Inspector → Add Component
   - "OneWayPlatform" 검색 후 추가

#### Step 3: 설정
```
OneWayPlatform 설정:
├─ Drop Through Duration: 0.3 (떨어질 때 무시할 시간)
└─ Player Layer: Player (플레이어 레이어 설정)
```

---

### 2. 일반 GameObject에 적용하는 경우:

#### Step 1: 플랫폼 오브젝트 생성
```
Hierarchy 우클릭:
└─ Create Empty
   이름: "OneWayPlatform"
```

#### Step 2: 컴포넌트 추가
1. **Collider 2D** 추가
   - Box Collider 2D 또는 원하는 Collider
   
2. **OneWayPlatform 스크립트** 추가

3. **Rigidbody 2D** 추가 (선택사항)
   - Body Type: Static

#### Step 3: 모양 만들기
```
자식으로 Sprite 추가:
└─ OneWayPlatform
   └─ Sprite (Sprite Renderer)
```

---

## ⚙️ Inspector 설정

### OneWayPlatform 컴포넌트:
```
📦 OneWayPlatform
├─ Drop Through Duration: 0.3
│  └─ S키 눌렀을 때 플랫폼을 무시할 시간 (초)
│  └─ 0.3초 권장
│
└─ Player Layer: Player
   └─ 플레이어의 레이어 설정
   └─ 선택사항 (자동 감지 가능)
```

---

## 🎯 작동 방식

### 1. 아래에서 위로 통과:
```
플레이어
  ↑ (점프)
━━━━━ 플랫폼
  ↑
→ 통과 가능! ✅
```

### 2. 위에서 밟기:
```
플레이어
━━━━━ 플랫폼 ← 밟을 수 있음!
  
→ 걸어다닐 수 있음 ✅
```

### 3. S키로 떨어지기:
```
플레이어 (S키 누름)
━━━━━ 플랫폼
  ↓
  
→ 0.3초간 플랫폼 통과
→ 아래로 떨어짐 ✅
```

---

## 🎮 컨트롤

| 입력 | 동작 |
|------|------|
| **점프** | 아래에서 위로 통과 |
| **S키** | 플랫폼 위에서 누르면 떨어짐 |
| **↓ 키** | S키와 동일 (떨어짐) |
| **이동** | 플랫폼 위에서 걸어다님 |

---

## 💡 주요 특징

### PlatformEffector2D 사용:
```
Unity의 내장 일방향 플랫폼 시스템 사용
- useOneWay: true
- surfaceArc: 180° (위쪽만 충돌)
- 자동으로 아래→위 통과 가능
```

### S키 떨어지기:
```
S키 감지 → Collider 비활성화 → 0.3초 후 재활성화
→ 플레이어가 떨어진 후 다시 밟을 수 있음
```

---

## 🔍 Scene View 시각화

### Gizmo 표시:
```
초록색 와이어 박스: 플랫폼 범위
빨간색 화살표: 위쪽 (밟을 수 있는 방향)
```

---

## 🎯 테스트 체크리스트

### 기본 테스트:
- [ ] 플레이어가 아래에서 점프하면 통과됨
- [ ] 플랫폼 위에서 걸어다닐 수 있음
- [ ] S키 누르면 떨어짐
- [ ] 0.3초 후 다시 플랫폼 밟을 수 있음

### Tilemap 테스트:
- [ ] Tilemap Collider 2D 설정됨
- [ ] OneWayPlatform 스크립트 추가됨
- [ ] 모든 타일에서 일방향 작동

---

## 🆘 문제 해결

### "플레이어가 통과하지 못해요"
```
1. PlatformEffector2D 확인
   - useOneWay: true
   - surfaceArc: 180
   
2. Collider 확인
   - usedByEffector: true
   
3. Rigidbody2D 확인
   - Body Type: Static
```

### "S키를 눌러도 떨어지지 않아요"
```
1. 플레이어가 플랫폼 위에 있는지 확인
2. Keyboard.current가 null이 아닌지 확인
3. Console에서 로그 확인
   "⬇️ 플랫폼 통과 시작" 메시지
```

### "떨어진 후 다시 못 올라와요"
```
Drop Through Duration 증가:
0.3 → 0.5

플레이어가 완전히 떨어질 때까지 시간 필요
```

---

## 🎊 완료!

### 설정 순서:
1. Tilemap 또는 GameObject 준비
2. Collider 2D 추가
3. OneWayPlatform 스크립트 추가
4. 테스트!

### 결과:
- ✅ 아래→위 통과 가능
- ✅ 위에서 밟을 수 있음
- ✅ S키로 떨어지기
- ✅ 자동으로 다시 밟을 수 있음

**완벽한 일방향 플랫폼!** 🎮✨

---

## 📝 추가 팁

### 여러 개 만들기:
```
1. 첫 번째 플랫폼 설정
2. Ctrl+D로 복제
3. 위치 조정
→ 설정이 그대로 복사됨!
```

### Tilemap에서 레이어 분리:
```
Grid
├─ Ground (일반 땅)
├─ OneWayPlatform (일방향) ← OneWayPlatform 스크립트
└─ Walls (벽)
```

### 색상으로 구분:
```
Tilemap Color를 다르게 설정:
- 일반 플랫폼: 흰색
- 일방향 플랫폼: 노란색
→ 플레이어가 구분 가능!
```

