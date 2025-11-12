# PathRecorder 사용 가이드

## 개요
PathRecorder는 플레이어의 이동 경로를 실시간으로 기록하고 라인으로 시각화하는 Unity 스크립트입니다.

## 설치 방법

### 1단계: 필수 에셋 생성 (Material & Prefab)

**방법 1: 자동 생성 (추천)**
1. Unity 상단 메뉴에서 `Tools > PathRecorder > Quick Setup (Default Settings)` 클릭
2. 완료! Material과 Prefab이 자동으로 생성됩니다.

**방법 2: 커스텀 설정으로 생성**
1. Unity 상단 메뉴에서 `Tools > PathRecorder > Setup Assets` 클릭
2. 라인 색상과 두께를 원하는 대로 조정
3. "에셋 생성하기" 버튼 클릭

생성된 파일 위치: `Assets/Platformer Toolkit Demo/Resources/`
- PathLineMaterial.mat
- PathLinePrefab.prefab

### 2단계: GameObject에 스크립트 추가

1. **GameObject에 스크립트 추가**
   - 씬에 빈 GameObject를 생성하거나 기존 GameObject에 추가
   - `PathRecorder` 스크립트를 추가

2. **설정 (선택사항)**
   - Main Camera가 자동으로 설정되지만, 필요시 수동으로 할당 가능
   - Line Renderer Prefab과 Line Material은 비워두면 자동으로 기본값 사용
   - 원하면 Resources 폴더의 에셋을 수동으로 할당 가능

## 사용 방법

### 기본 조작
- **자동 기록**: 게임 시작 시 자동으로 플레이어 경로 기록이 시작됩니다
- **좌클릭**: 경로 기록을 시작/정지합니다 (토글 방식)
- **우클릭**: 마지막으로 기록된 경로 라인을 삭제합니다
- **플레이어 이동**: 플레이어가 움직이면 자동으로 경로가 라인으로 그려집니다

### Inspector 설정

#### 대상 설정
- **Player Transform**: 추적할 플레이어의 Transform
  - 비워두면 스크립트가 붙은 GameObject 자신을 추적합니다
  - 다른 오브젝트를 추적하려면 해당 Transform을 드래그 앤 드롭

#### 라인 설정
- **Line Renderer Prefab**: (선택사항) 미리 만든 LineRenderer 프리팹
- **Line Color**: 라인의 색상 (기본: 흰색)
- **Line Width**: 라인의 두께 (기본: 0.1)
- **Line Material**: (선택사항) 라인에 사용할 머티리얼

#### 기록 설정
- **Min Distance Between Points**: 점 사이의 최소 거리 (기본: 0.1)
  - 값이 작을수록 더 부드러운 라인 (더 자주 기록)
  - 값이 클수록 더 각진 라인 (덜 자주 기록)
- **Record On Start**: 게임 시작 시 자동으로 기록 시작 (기본: 체크됨)

#### 디버그
- **Show Debug Info**: 콘솔에 디버그 정보 출력

## 코드에서 사용하기

```csharp
using GMTK.PlatformerToolkit;

// PathRecorder 참조
PathRecorder pathRecorder = GetComponent<PathRecorder>();

// 수동으로 기록 시작
pathRecorder.StartRecording();

// 기록 정지
pathRecorder.StopRecording();

// 모든 라인 삭제
pathRecorder.ClearAllLines();

// 마지막 라인만 삭제
pathRecorder.UndoLastLine();

// 기록된 모든 경로 가져오기
List<List<Vector3>> allPaths = pathRecorder.GetAllRecordedPaths();
```

## 추가 기능 아이디어

향후 추가할 수 있는 기능:
- 라인을 따라 다시 플레이 (리플레이) 기능
- 경로 비교 기능 (예: 최적 경로 vs 플레이어 경로)
- 라인 저장/불러오기
- 다양한 라인 스타일 (점선, 화살표 등)
- 라인 페이드 아웃 효과
- 특정 조건에서만 기록 (예: 점프 중일 때만)

## 활용 예시

- **타임어택 게임**: 최적 경로와 비교
- **퍼즐 게임**: 이동 경로 확인
- **교육용 게임**: 플레이어의 학습 패턴 분석
- **리플레이 시스템**: 이전 플레이 기록 표시
- **고스트 레이싱**: 이전 기록과 경쟁

## 주의사항

- 너무 작은 `minDistanceBetweenPoints` 값은 성능에 영향을 줄 수 있습니다
- 매우 긴 경로는 메모리를 많이 사용할 수 있습니다
- 여러 개의 경로를 동시에 기록하면 성능 저하가 발생할 수 있습니다

