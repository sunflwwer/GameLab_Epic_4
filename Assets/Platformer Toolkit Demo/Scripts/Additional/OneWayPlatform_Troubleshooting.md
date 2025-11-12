# 일방향 플랫폼 문제 해결 가이드 🔧

## 🚨 현재 문제

- ❌ 아래→위 통과 안 됨
- ❌ S키 떨어지기 안 됨
- ✅ 위에서 걷기는 됨 (Ground 레이어)

---

## 🔍 문제 원인 체크

### 1. Rigidbody2D 확인
```
Tilemap 오브젝트 선택 → Inspector 확인:

Rigidbody2D:
├─ Body Type: Static ✅ (반드시!)
├─ Simulated: ✅ 체크
└─ Use Auto Mass: ✅ 체크
```

### 2. Collider 확인
```
Tilemap Collider 2D:
└─ Used By Effector: ✅ 반드시 체크!

또는

Composite Collider 2D:
├─ Geometry Type: Polygons
└─ Used By Effector: ✅ 반드시 체크!
```

### 3. PlatformEffector2D 확인
```
PlatformEffector2D:
├─ Use One Way: ✅ 체크
├─ Surface Arc: 180
├─ Side Arc: 0
├─ Use Collider Mask: ❌ 체크 해제!
└─ Rotational Offset: 0
```

---

## 📋 올바른 설정 순서

### Tilemap에 적용:

#### Step 1: 컴포넌트 추가 (순서 중요!)
```
1. Tilemap Collider 2D 추가
2. Composite Collider 2D 추가 (선택사항)
   → 자동으로 Rigidbody2D 추가됨
3. Rigidbody2D를 Static으로 설정
4. OneWayPlatform 스크립트 추가
```

#### Step 2: Collider 설정
```
Tilemap Collider 2D:
└─ Used By Composite: ✅ (Composite 사용 시)

Composite Collider 2D:
├─ Geometry Type: Polygons
└─ Used By Effector: ✅ 반드시!
```

#### Step 3: 확인
```
Hierarchy에서 Tilemap 선택 → Inspector:

✅ Tilemap Collider 2D
✅ Composite Collider 2D
✅ Rigidbody 2D (Static)
✅ Platform Effector 2D (자동 추가됨)
✅ OneWayPlatform
```

---

## 🎯 테스트 방법

### Play 모드에서:

1. **Console 창 열기** (Ctrl+Shift+C)
2. **플레이어로 플랫폼 아래에서 점프**
3. **Console 확인:**
   ```
   ✅ OneWayPlatform 초기화 완료: Tilemap
      - Collider Type: CompositeCollider2D
      - UsedByEffector: True
      - UseOneWay: True
      - SurfaceArc: 180
   ```

4. **플랫폼 위에 착지**
5. **Console 확인:**
   ```
   🔵 플레이어 충돌: Player → Tilemap
   🔄 떨어지기 가능 상태: True (높이 차: 0.5)
   ```

6. **S키 누르기**
7. **Console 확인:**
   ```
   ⬇️ S키 감지 - 떨어지기 시작!
   ⬇️ 플랫폼 통과 시작: Tilemap, 0.3초간
   ```

---

## 🆘 여전히 안 되는 경우

### 문제 1: 아래→위 통과 안 됨

#### 체크리스트:
- [ ] Rigidbody2D가 **Static**인가?
- [ ] Collider의 **Used By Effector** 체크됨?
- [ ] PlatformEffector2D의 **Use One Way** 체크됨?
- [ ] PlatformEffector2D의 **Surface Arc**가 180인가?

#### 해결 방법:
```
1. PlatformEffector2D 제거
2. OneWayPlatform 스크립트 제거
3. 다시 OneWayPlatform 스크립트 추가
   → 자동으로 올바르게 설정됨
```

### 문제 2: S키 떨어지기 안 됨

#### 체크리스트:
- [ ] 플레이어가 플랫폼 **위**에 있는가?
- [ ] Console에 "🔵 플레이어 충돌" 메시지가 나오는가?
- [ ] Console에 "🔄 떨어지기 가능 상태: True" 메시지가 나오는가?
- [ ] S키를 누를 때 "⬇️ S키 감지" 메시지가 나오는가?

#### 해결 방법:
```
1. OneWayPlatform 스크립트의 Drop Through Duration을 0.5로 증가
2. 플레이어의 Collider가 Ground와 충돌하는지 확인
3. Input System이 제대로 작동하는지 확인
```

---

## 💡 추가 설정

### Physics2D Settings 확인:
```
Edit → Project Settings → Physics 2D:

Queries Hit Triggers: ❌ (일반적으로 체크 해제)
```

### Layer Collision Matrix:
```
Edit → Project Settings → Physics 2D → Layer Collision Matrix:

Ground 레이어와 Player 레이어가 충돌하도록 설정 ✅
```

---

## 🎊 완전한 Inspector 설정 예시

```
📦 Tilemap (Hierarchy)
├─ Transform
├─ Tilemap
├─ Tilemap Renderer
├─ Tilemap Collider 2D
│  └─ Used By Composite: ✅
├─ Composite Collider 2D
│  ├─ Geometry Type: Polygons
│  └─ Used By Effector: ✅
├─ Rigidbody 2D
│  ├─ Body Type: Static ✅
│  └─ Simulated: ✅
├─ Platform Effector 2D (자동 추가)
│  ├─ Use One Way: ✅
│  ├─ Surface Arc: 180
│  ├─ Side Arc: 0
│  └─ Use Collider Mask: ❌
└─ OneWayPlatform
   ├─ Drop Through Duration: 0.3
   └─ Player Layer: Nothing (자동 감지)
```

---

## 📝 디버그 로그로 확인

### 올바른 로그 순서:
```
1. 게임 시작:
   ✅ OneWayPlatform 초기화 완료
   
2. 플랫폼 위 착지:
   🔵 플레이어 충돌
   🔄 떨어지기 가능 상태: True
   
3. S키 누름:
   ⬇️ S키 감지 - 떨어지기 시작!
   ⬇️ 플랫폼 통과 시작
   
4. 0.3초 후:
   💚 플랫폼 충돌 복구
```

### 로그가 안 나온다면:
- **초기화 로그 없음** → 스크립트가 실행 안 됨
- **충돌 로그 없음** → Collider 설정 문제
- **S키 로그 없음** → Input System 문제
- **통과 시작 로그 없음** → canDropThrough가 false

---

## 🔧 빠른 해결 방법

### 방법 1: 처음부터 다시 설정
```
1. 모든 컴포넌트 제거:
   - Tilemap Collider 2D
   - Composite Collider 2D
   - Rigidbody 2D
   - Platform Effector 2D
   - OneWayPlatform

2. OneWayPlatform만 추가
   → 다른 것들 자동 설정

3. Composite Collider 2D 수동 추가
   → Used By Effector 체크

4. Tilemap Collider 2D에서
   → Used By Composite 체크
```

### 방법 2: 새 Tilemap으로 테스트
```
1. 새 Tilemap 생성
2. 타일 1~2개만 배치
3. OneWayPlatform 추가
4. 테스트
   → 작동하면 기존 Tilemap 설정 문제
```

---

## ✅ 최종 체크리스트

Play 모드에서 Console 확인:
- [ ] ✅ OneWayPlatform 초기화 완료
- [ ] 🔵 플레이어 충돌 (착지 시)
- [ ] 🔄 떨어지기 가능 상태: True
- [ ] ⬇️ S키 감지 (S키 누를 때)
- [ ] ⬇️ 플랫폼 통과 시작
- [ ] 💚 플랫폼 충돌 복구 (0.3초 후)

**모든 로그가 나오면 정상 작동!** ✅

