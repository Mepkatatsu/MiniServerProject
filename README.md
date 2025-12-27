# MiniServerProject

게임 서버 컨텐츠 로직(스테이지 Enter/Clear/GiveUp) 구현을 통해 서버 중심의 **상태 전이 기반 API**, **멱등성(Idempotency)**, **운영 관점 예외/로그** 처리를 목적으로 한 프로젝트입니다.

실제 게임 서버에서 자주 맞닥뜨리는 문제(재시도/중복 요청, 동시성, 상태 정합성, 운영 장애 대응)를 최소 범위로 구현/검증하는 것을 목표로 했습니다.

---

## 핵심 포인트

### 1) 상태 전이 기반 API 설계
스테이지 컨텐츠를 “유저 상태 전이”로 표현합니다.

- `POST /stages/{stageId}/enter` : 스테이지 입장 (스태미너 소비 + CurrentStageId 설정)
- `POST /stages/{stageId}/clear` : 스테이지 클리어 (보상 지급 + CurrentStageId 해제)
- `POST /stages/{stageId}/give-up` : 스테이지 포기 (스태미너 일부 환불 + CurrentStageId 해제)

유저는 다음 API로 관리합니다.

- `POST /users` : 유저 생성 (AccountId 기반 멱등성)
- `GET /users/{accountId}` : 유저 조회

---

### 2) 실서비스 패턴의 멱등성 처리 (Redis + DB Log + UNIQUE)
모든 “치명적인 중복 요청이 발생할 수 있는” POST 요청에 대해 멱등성을 적용했습니다.

**정합성의 최종 보장은 DB 로그 + UNIQUE 제약으로 처리**하고, Redis는 빠른 응답을 위한 캐싱 시스템으로 사용합니다.

공통 흐름은 다음과 같습니다.

1. **Redis 캐시 조회** (있으면 즉시 동일 응답 반환)
2. **DB 로그 선조회** (이미 처리된 요청이면 동일 응답 반환)
3. **실제 처리 + DB 로그 INSERT**
4. **UNIQUE 충돌 발생 시(동시성/중복 요청)** → DB 로그 재조회 후 동일 응답 반환
5. 최종 응답을 **Redis에 캐싱**

추가로 Stage API에서는 `RequestId` 재사용 방어를 수행합니다.

- 동일 `(UserId, RequestId)`가 이미 사용되었는데 `stageId`가 다르면  
  → `409 Conflict (RequestIdUsedForDifferentStage)` 반환

---

### 3) 서버 중심의 상태 변경
유저의 스태미너/스테이지 상태/재화 변화는 **User 엔티티**에서 관리합니다.

- `UpdateStaminaByDateTime()`
  - `LastStaminaUpdateTime` 기반으로 경과 시간을 계산하고, 회복 주기(`GameParameters.StaminaRecoverCycleSec`)에 맞춰 자동 회복
  - 회복 가능한 최대치(`StaminaTable: MaxRecoverableStamina`)를 넘지 않도록 제한
- `ConsumeStamina() / AddStamina()`
  - 회복 반영 → 소비/획득 적용
- `SetCurrentStage() / ClearCurrentStage(stageId)`
  - 잘못된 stageId로 Clear 시도 시 예외로 방어

즉, 서비스/컨트롤러에서 임의로 상태를 조작하지 않고 **도메인 규칙을 통해 상태 전이를 통제**합니다.

---

### 4) 정적 테이블(Static Table) 기반 컨텐츠 로직
컨텐츠 수치/규칙을 코드 로직과 분리하기 위해 정적 테이블 구조를 구성했습니다.

- `StageTable` : 스테이지 요구 스태미너, 보상 ID
- `RewardTable` : 보상(골드/경험치)
- `StaminaTable` : 레벨별 회복 가능한 최대 스태미너
- `GameParameters` : 회복 주기, 포기 환불 비율(Refund Rate)

공통 인터페이스 + 제네릭 테이블 베이스를 사용하여 확장 가능하도록 구성했습니다.

- `ITable`, `ITable<TKey, TData>`
- `TableBase<TKey, TData>`
- `TableHolder.GetTable<T>()` (Lazy init + 캐싱)

---

### 5) 운영 관점 예외 처리: 전역 미들웨어 + 표준 에러 응답
컨트롤러에서 try-catch를 제거하고, 전역 미들웨어에서 예외를 처리합니다.

- `DomainException(ErrorType)` → HTTP Status + 메시지 매핑
- 기타 예외 → `500 InternalServerError` + 서버 로그 기록

에러 응답은 다음 형태로 통일했습니다.

- `error`, `message`, `traceId`, `details`

---

## 기술 스택
- .NET 8 / ASP.NET Core Web API
- EF Core + MySQL
- Redis
- xUnit
- Swagger

---

## 프로젝트 구조(요약)
[MiniServerProject]
- `Api/`
  - `Middleware/ExceptionHandlingMiddleware` : 전역 예외 처리
  - `Common/ApiErrorResponse` : 표준 에러 응답
- `Application/`
  - `UserService`, `StageService`
  - `IdempotencyKeyFactory` : 멱등성 키 규칙
  - `DomainException`, `ErrorType`
- `Controllers/`
  - `UsersController`, `StagesController`
- `Domain/`
  - `Entities/User` : 서버 중심 상태 전이
  - `ServerLogs/*` : 멱등성/운영을 위한 로그 테이블
- `Infrastructure/`
  - `Persistence` : GameDbContext + EF Configurations
  - `Redis/RedisCache` : `IIdempotencyCache` 구현

[MiniServerProject.Shared] : 서버-클라이언트 간 공유하는 코드
  - `Requests/`
  - `Responses/`
  - `Tables/` : 정적 테이블 구조

---

## 테스트
서비스 레이어 단위 테스트를 통해 정상/예외/멱등성 시나리오를 검증합니다.

- User
  - 생성 성공
  - 동일 AccountId 재요청 시 동일 응답(멱등성)
  - 캐시 Hit 시 DB 접근하지 않음
  - invalid 요청(공백) 예외
  - 조회 실패(UserNotFound)
- Stage
  - Enter 성공(스태미너 소비 + 로그)
  - Enter 멱등성(동일 RequestId)
  - Clear 성공(보상 지급 + 스테이지 종료 + 로그)
  - Clear 멱등성(동일 RequestId)
  - GiveUp 성공(일부 환불 + 스테이지 종료 + 로그)
  - GiveUp 멱등성(동일 RequestId)
  - 스태미너 부족/Enter 없이 Clear/GiveUp 실패 케이스
  - Stage 정보가 없는 경우에도 GiveUp은 성공(유저 상태 정리 목적)

---

## 테스트 클라이언트

간편하게 기능을 테스트해볼 수 있도록 콘솔 기반 테스트 클라이언트를 함께 구성했습니다.

- 로그인 / 유저 생성
- 유저 정보 표시
- 스테이지 진입 / 클리어 / 포기 시나리오

테스트 클라이언트는 의도적으로 단순한 구조를 유지하며, 클라이언트 측 입력 검증 및 예외 처리는 최소화했습니다.
이는 클라이언트가 불완전한 상태에서도 서버 로직이 안정적으로 동작하는지 확인하는 데 초점을 두었기 때문입니다.

---

## 실행 방법

(서버)
1. MySQL 준비 및 ConnectionString 설정 (`MiniServerProject\appsettings.json`의 `GameDb`)
2. Redis 설정 및 실행 (필수 X, `MiniServerProject\appsettings.json`의 `Redis`, `기본) localhost:6379`)

```bash

3. run-dev-with-migration.bat 파일 실행

혹은

dotnet ef database update --project MiniServerProject\MiniServerProject.csproj
start "MiniServerProject" dotnet run --project MiniServerProject\MiniServerProject.csproj --launch-profile "https"
start "" https://localhost:7165/swagger

```

(테스트)
```bash

test-dev-with-migration.bat 실행

혹은

dotnet ef database update --project MiniServerProject\MiniServerProject.csproj
dotnet test

```

(테스트 클라이언트)
```bash

run-testclient-dev.bat 파일 실행

혹은

dotnet run --project MiniServerProject.TestClient\MiniServerProject.TestClient.csproj

```
