using Core.Domain;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.GraphQLTypes
{
    public class DiagnosisType : ObjectGraphType<Diagnosis>
    {
        public DiagnosisType()
        {
            Field(x => x.Id);
            Field(x => x.Code);
            Field(x => x.BodyArea);
            Field(x => x.Pathology);
        }
    }
}
