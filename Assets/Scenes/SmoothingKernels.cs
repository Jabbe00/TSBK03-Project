using UnityEngine;

public class SmoothingKernels : MonoBehaviour
{
    public float h;
    public float h2;   // h^2
    public float h3;   // h^3
    public float h6;   // h^6
    public float h9;   // h^9

    public void SetRadius(float radius)
    {
        h = radius;
        h2 = h * h;
        h3 = h2 * h;
        h6 = h3 * h3;
        h9 = h6 * h3;

        poly6Constant = 315f / (64f * Mathf.PI * h9);
        spikyConstant = 15f / (Mathf.PI * h6);
        spikyGradConstant = -45f / (Mathf.PI * h6);
        viscConstant = 15f / (2f * Mathf.PI * h3);
        viscLaplacConstant = 45f / (Mathf.PI * h6);
    }

    // Kernel constants
    private float poly6Constant;
    private float spikyConstant;
    private float spikyGradConstant;
    private float viscConstant;
    private float viscLaplacConstant;

    public float W_ploy6(Vector3 r)
    {
        float r2 = r.magnitude;
        //USE FOR ALL BUT PRESSURE AND VISCOSITY
        if (r2 >= 0 && r2 <= h)
        {
            //return 315f / (64f * Mathf.PI * Mathf.Pow(h, 9f)) * Mathf.Pow(h*h - r2*r2, 3f);
            return poly6Constant * Mathf.Pow(h2 - r2*r2, 3f);
        }
        return 0;
    }

    public float W_spiky(Vector3 r)
    {
        float r2 = r.magnitude;
        //USE FOR PRESSURE
        if (r2 >= 0 && r2 <= h)
        {
            //return 15f / (Mathf.PI * Mathf.Pow(h, 6f)) * Mathf.Pow(h - r2,3f);
            return spikyConstant * Mathf.Pow(h - r2, 3f);
        }
        return 0;
    }

    public Vector3 gradientW_spiky(Vector3 r)
    {
        float r2 = r.magnitude;
        //CALCULATED MYSELF; MATCHES FIGURE IN PAPER, BUT FOR ACTUAL DERIVATIVE ADD A MINUS SIGN
        if(r2 >= 1e-6f && r2 <= h)
        {
            //return (45 / (Mathf.PI * Mathf.Pow(h, 6))) * Mathf.Pow(h - r2, 2);
            return -spikyGradConstant * Mathf.Pow(h - r2, 2) * r/r2;
        }
        return Vector3.zero;
        
    }

    public float W_viscosity(Vector3 r)
    {
        float r2 = r.magnitude;
        //USE FOR VISCOSITY
        if (r2 >= 0 && r2 <= h)
        {
            //return 15f / (2f * Mathf.PI * Mathf.Pow(h, 3f)) *(-(Mathf.Pow(r2, 3f) / (2 * Mathf.Pow(h, 3f))) +(Mathf.Pow(r2, 2f) / Mathf.Pow(h, 2f)) + (h / (2 * r2)) - 1);

            return viscConstant *
                (-(Mathf.Pow(r2, 3f) / (2 * h3)) +
                (Mathf.Pow(r2, 2f) / h2) + (h / (2 * r2)) - 1);
        }
        return 0;
    }

    public float laplacianW_viscosity(Vector3 r)
    {
        float r2 = r.magnitude;
        //DIVIDED BY 10 TO MAKE IT MATCH FIGURE IN THE PAPER, MIGHT NOT BE CORRECT
        if (r2 >= 0 && r2 <= h)
        {
            //return ((45 / (Mathf.PI * Mathf.Pow(h, 6))) * (h - r2))/10;
            return (viscLaplacConstant * (h - r2))/ 10;
        }
        return 0;
    }
}
