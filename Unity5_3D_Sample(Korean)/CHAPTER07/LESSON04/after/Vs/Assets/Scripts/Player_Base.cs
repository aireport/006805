using UnityEngine;
using System.Collections;




/*
 *	플레이어 클래스 기저
 *	Maruchu
 *
 *	캐릭터 이동, 메카님(메션)제어 등
 */
public		class		Player_Base				: HitObject {




// 플레이어 조작 종류
protected	enum	PlayerInput {
	 Move_Left		// 이동 왼쪽
	,Move_Up		// 이동 위쪽
	,Move_Right		// 이동 오른쪽
	,Move_Down		// 이동 아래쪽
	,Shoot			// 사격
	,EnumMax		// 전체 조작 개수
}

private		static readonly		float		MOVE_ROTATION_Y_LEFT	= -90f;		// 이동 방향 왼쪽
private		static readonly		float		MOVE_ROTATION_Y_UP		=   0f;		// 이동 방향 위쪽
private		static readonly		float		MOVE_ROTATION_Y_RIGHT	=  90f;		// 이동 방향 오른쪽
private		static readonly		float		MOVE_ROTATION_Y_DOWN	= 180f;		// 이동 방향 아래쪽

public							float		MOVE_SPEED				= 5.0f;		// 이동 속도





public							GameObject	playerObject			= null;		// 움직일 대상 모델
public							GameObject	bulletObject			= null;		// 총알 프리팹


public							GameObject	hitEffectPrefab			= null;		// 히트 효과 프리팹





private							float		m_rotationY				= 0.0f;		// 플레이어 회전 각도

protected						bool[]		m_playerInput			= new bool[ (int)PlayerInput.EnumMax];		// 키를 누르고 있다

protected						bool		m_playerDeadFlag		= false;		// 플레이어 사망 여부를 정하는 플래그




/*
 *	각 프레임에서 호출되는 함수
 */
private		void	Update() {

	// 플레이어가 사망한 상태
	if( m_playerDeadFlag) {
		// 모든 처리를 무시한다
		return;
	}

	// 플래그 초기화
	ClearInput();
	// 입력 처리를 얻는다
	GetInput();

	// 이동 처리
	CheckMove();
}


/*
 *	입력 처리 검사
 */
private			void	ClearInput() {
	// 플래그 초기화
	int	i;
	for( i=0; i<(int)PlayerInput.EnumMax; i++) {
		m_playerInput[ i]	= false;
	}
}
/*
 *	입력 처리 검사
 */
protected	virtual	void	GetInput() {
}


/*
 *	이동 처리 검사
 */
private			void	CheckMove() {

	// 애니메이터(메카님)를 얻는다
	Animator	animator	= playerObject.GetComponent<Animator>();

	// 총알에 맞지 않았으면 이동 가능
	float	moveSpeed	= MOVE_SPEED;		// 이동 속도
	bool	shootFlag	= false;			// 총알을 쏘는지 여부를 나타내는 플래그

	// 이동과 회전
	{
		// 키 조작으로 회전과 이동
		if( m_playerInput[ (int)PlayerInput.Move_Left]) {
			// 왼쪽
			m_rotationY		= MOVE_ROTATION_Y_LEFT;
		} else
		if( m_playerInput[ (int)PlayerInput.Move_Up]) {
			// 위
			m_rotationY		= MOVE_ROTATION_Y_UP;
		} else
		if( m_playerInput[ (int)PlayerInput.Move_Right]) {
			// 오른쪽
			m_rotationY		= MOVE_ROTATION_Y_RIGHT;
		} else
		if( m_playerInput[ (int)PlayerInput.Move_Down]) {
			// 아래
			m_rotationY		= MOVE_ROTATION_Y_DOWN;
		} else {
			// 아무것도 누르지 않았으면 이동하지 않는다
			moveSpeed		= 0f;
		}

		// 플레이어가 향할 방향을 오일러 각으로 입력한다
		transform.rotation	= Quaternion.Euler( 0, m_rotationY, 0);
        // Y축 회전으로 캐릭터의 방향을 옆으로 바꾼다

		// 이동량을 Transform에 넘겨주어 이동시킨다
		transform.position	+= ((transform.rotation	 	*(Vector3.forward	*moveSpeed))		*Time.deltaTime);
	}

	// 사격
	{
		// 사용자가 사격 버튼(클릭)을 눌렀는지 검사
		if( m_playerInput[ (int)PlayerInput.Shoot]) {
			// 총알을 쏘았다
			shootFlag	= true;

			// 총알을 생성할 위치
			Vector3 vecBulletPos	= transform.position;
			// 진행 방향으로 조금 앞쪽을 지정
			vecBulletPos			+= (transform.rotation	*Vector3.forward);
			// Y는 높이를 대충 올린다
			vecBulletPos.y			= 2.0f;

			// 총알을 생성한다
			Instantiate( bulletObject, vecBulletPos, transform.rotation);
		} else {
			// 쏘지 않았다
			shootFlag	= false;
		}
	}


	// 메카님
	{
		// 값을 Animator로 넘겨준다
		animator.SetFloat(	"Speed",	moveSpeed);		// 이동량
		animator.SetBool(	"Shoot",	shootFlag);		// 사격 플래그
	}
}




/*
 *	Collider가 무언가에 닿으면 호출되는 함수
 *
 *	자신의 GameObject에 Collider(IsTrigger를 지정)와 Rigidbody를 설정하면 호출된다
 */
private		void	OnTriggerEnter( Collider hitCollider) {

	// 닿아도 되는지 확인한다
	if( false==IsHitOK( hitCollider.gameObject)) {
		// 이 오브젝트에 닿아서는 안된다
		return;
	}

	// 총알에 맞았다
	{
		// 애니메이터(메카님)를 얻는다
		Animator	animator	= playerObject.GetComponent<Animator>();

		// 사망했다는 사실을 메카님에 알린다
		animator.SetBool(	"Dead",		true);		// 사망 플래그
	}

	// 히트 효과가 있는지 판정한다
	if( null!=hitEffectPrefab) {
		// 자신의 현재 위치에서 히트 효과를 낸다
		Instantiate( hitEffectPrefab, transform.position, transform.rotation);
	}

	// 이 플레이어는 죽은 상태로 설정한다
	m_playerDeadFlag	= true;
}




}
