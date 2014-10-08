using UnityEngine;
using System.Collections;

public class traceHand : MonoBehaviour
{

		public GameObject hand;//hand object
		public ParticleSystem stars; //particle system created by unity
		public RegretFHParticle[] mirrorList;//created from our regretParticle
		public ParticleSystem.Particle[] regretParticleCopy;
		public Vector3[] handTrace;//record hand poisitions
		public Vector3 handDirection;//hand moving vectors
		public Vector3 handAcceleration = new Vector3 (0, 0, 0);//average hand moving acceleration per frame
		public int handTraceLength = 20;//how many particles we store for calcuating the average acceleration
		public int counterForHandTracingList = 0;
		bool emitted;

		void Start ()
		{
				emitted = false;
			if(gameObject.name == "Particle System left"){
			hand = GameObject.Find ("HandLeft");
		}else if(gameObject.name == "Particle System"){
			hand = GameObject.Find ("HandRight");
		}
				stars = GetComponent<ParticleSystem> ();
				regretParticleCopy = new ParticleSystem.Particle[stars.maxParticles];
				mirrorList = new RegretFHParticle[stars.maxParticles];//init
				for(int i=0;i<mirrorList.Length;i++){
					mirrorList[i] = new RegretFHParticle(new Vector3(0,0,0),new Vector3(0,0,0) );
				}
		}

		// Update is called once per frame
		void LateUpdate ()
		{


	

				if (!emitted) {

						print ("check emitting status...");
						
						if (stars.particleCount == stars.maxParticles) {//check if we got all of the particles, 

								
								stars.GetParticles (regretParticleCopy); //regretParticleCopy got all data.
							
			
								for (int i=0; i < stars.maxParticles; i++) {

										mirrorList [i].position = regretParticleCopy [i].position;
										mirrorList [i].velocity = regretParticleCopy [i].velocity;

								}


								//hand tracing stuff
								handTrace = new Vector3[handTraceLength];

								emitted = true;
								print ("loop start!");
						}

				} else {


						//hand stuff
						handTracing ();
						gethandAcceleration ();
				
						//calculate
						print ("size of rgparticlelist: " + mirrorList.Length);
					
						for (int i=0; i< mirrorList.Length; i++) {
								mirrorList [i].updateHandAcceleration (handAcceleration);
								mirrorList [i].calculate ();
								mirrorList [i].backToOriginalPath ();
						}

						//apply all calculated values to unity's particleSystem
						for (int i=0; i <  stars.maxParticles; i++) {
								regretParticleCopy [i].position = mirrorList [i].position;
								regretParticleCopy [i].velocity = mirrorList [i].velocity;
								//print ("number "+ i  +"'s position is: " + mirrorList[i].position);
								//print ("number "+ i  +"'s velocity is: " + mirrorList[i].velocity);
						}



						//apply all calculated values to unity's particleSystem AGAIN
						stars.SetParticles (regretParticleCopy, stars.maxParticles);

						handAcceleration.Set (0, 0, 0); //clear handAcceleration, get ready for the next loop

				}
		}

		public void handTracing ()
		{


				//cap array length
				if (counterForHandTracingList > handTraceLength - 1) {
						counterForHandTracingList = 0;
				}


				//get hand positions and save them into an array

				handTrace [counterForHandTracingList] = new Vector3 (hand.transform.position.x, hand.transform.position.y, 0);
				counterForHandTracingList++;

		}

		public void gethandAcceleration ()
		{

				//sum up all variable in hand tracing list and get the average acceleration value for shitting particles
				for (int i=0; i< counterForHandTracingList-1; i++) {
						handAcceleration += handTrace [i + 1] - handTrace [i];
				}

				handAcceleration /= counterForHandTracingList;

		}


	public class RegretFHParticle
		{
				public Vector3 position;
				public Vector3 target;
				public Vector3 originalTarget;
				public Vector3 acceleration;
				public Vector3 velocity;
				Vector3 impactedForce;
				float maxspeed;
				float maxforce;
				float backToOriginForce;

				public RegretFHParticle (Vector3 _position, Vector3 _velocity)
				{
						position = _position;
						target = new Vector3 (position.x, position.y - 900, position.z); //pick a target
						originalTarget = new Vector3 (position.x, position.y - 900, position.z);//store the original(destined) target
						velocity = _velocity;
						acceleration = new Vector3 (0, 0, 0);
						maxspeed = 0.1f;
						backToOriginForce = 4;
						maxforce = 4;
				}

				public void updateHandAcceleration (Vector3 handAcceleration)
				{
						Vector3 md = handAcceleration;
						md.Normalize ();
						md *= 50;
						target += md;
				}

				public void calculate ()
				{
						Vector3 desired = target - position;
						desired.Normalize ();
						desired *= maxspeed;
						Vector3 steer = desired - velocity;
						steer = Vector3.ClampMagnitude (steer, maxforce);
						acceleration += steer;
						velocity += acceleration;
						position += velocity;
						acceleration *= 0;
				}

				public void backToOriginalPath ()
				{
						if (target != originalTarget) {
								Vector3 desired = originalTarget - target;
								desired.Normalize ();
								desired *= backToOriginForce;
								target += desired;
						}
				}
		}
}




